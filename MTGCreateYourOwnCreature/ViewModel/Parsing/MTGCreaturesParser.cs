using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;

using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Model.Cards;
using MTGCreateYourOwnCreature.Model.Category;
using MTGCreateYourOwnCreature.ViewModel.Helpers;

namespace MTGCreateYourOwnCreature.ViewModel.Parsing
{
    /// <summary>
    /// Provides functionality to read, parse, and export creature cards using the custom text format of the editor.
    /// Converts unstructured string data into populated <see cref="MTGCreatureCard"/> data models, and vice versa.
    /// </summary>
    public class MTGCreaturesParser
    {
        /// <summary>
        /// Represents a method that extracts specific property data from a raw text block and applies it to a card model.
        /// </summary>
        /// <param name="card">The card data model being populated.</param>
        /// <param name="data">The raw string value or multi-line block extracted from the file.</param>
        /// <param name="cards">The list of previously parsed cards, used to resolve inheritance references.</param>
        public delegate void MTGParseAction(MTGCreatureCard card, string data, List<MTGCreatureCard> cards);

        /// <summary>
        /// Represents a method that serializes a specific property of a card model into the custom text format.
        /// </summary>
        /// <param name="card">The card data model being exported.</param>
        /// <param name="exportKey">The top-level text key (e.g., "mana", "stats") associated with this data block.</param>
        /// <param name="output">The running string builder accumulating the text output for the file.</param>
        public delegate void MTGExportAction(MTGCreatureCard card, string exportKey, StringBuilder output);

        /// <summary>
        /// A routing dictionary that maps specific top-level text file keys (e.g., "mana", "stats") 
        /// to the corresponding action responsible for parsing that segment's data into the card model.
        /// </summary>
        protected static readonly Dictionary<string, Action<MTGCreatureCard, string, List<MTGCreatureCard>>> ms_ParseActions = new Dictionary<string, Action<MTGCreatureCard, string, List<MTGCreatureCard>>>()
        {
            { "card", CardName },
            { "inherits", CardInherits },
            { "category", CardCategory },
            { "mana", CardMana },
            { "stats", CardStats },
            { "traits", CardTraits },
            { "text", CardText },
        };

        /// <summary>
        /// A routing dictionary that maps top-level text file keys to the corresponding action 
        /// responsible for formatting and writing that segment's data out to the text file.
        /// </summary>
        protected static readonly Dictionary<string, MTGExportAction> ms_ExportActions = new Dictionary<string, MTGExportAction>()
        {
            { "card", ExportCardName },
            { "inherits", ExportParentCard },
            { "category", ExportCategory },
            { "mana", ExportMana },
            { "stats", ExportStats },
            { "traits", ExportTraits },
            { "text", ExportText }
        };

        /// <summary>
        /// Reads a specified text file and parses every valid creature card entry found within it.
        /// </summary>
        /// <param name="filePath">The absolute or relative file path to the custom text file to be parsed.</param>
        /// <returns>A list of fully populated <see cref="MTGCreatureCard"/> objects representing the parsed file contents. Returns an empty list if the file is empty or missing.</returns>
        public static List<MTGCreatureCard> Parse(string filePath)
        {
            List<MTGCreatureCard> cards = new List<MTGCreatureCard>();

            string fileText = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(fileText))
            {
                return cards;
            }

            // Cards are separated by three dashes surrounded by blank lines.
            // We split the entire document into discrete card chunks based on this strict delimiter.
            string[] cardsText = Regex.Split(fileText.Trim(), @"\r?\n\r?\n---\r?\n\r?\n", RegexOptions.Multiline);
            for (int i = 0; i < cardsText.Length; ++i)
            {
                if (string.IsNullOrWhiteSpace(cardsText[i]))
                {
                    continue;
                }

                // Parse the individual text chunk and append the resulting card to our list.
                // We pass the running 'cards' list so that subsequent cards can look up previous ones for inheritance.
                cards.Add(CreateMTGCreatureCard(cardsText[i], cards));
            }

            return cards;
        }

        /// <summary>
        /// Asynchronously serializes a list of creature cards into the custom text format and writes them to disk.
        /// </summary>
        /// <param name="cards">The chronologically sorted list of creature cards to export.</param>
        /// <param name="filePath">The destination file path where the output will be saved.</param>
        /// <returns>A task representing the asynchronous file write operation.</returns>
        public static async Task Export(List<MTGCreatureCard> cards, string filePath)
        {
            StringBuilder exportData = new StringBuilder();

            const string separator = "\r\n\r\n---\r\n\r\n";

            int numberOfCards = cards.Count;

            for (int i = 0; i < numberOfCards; ++i)
            {
                // Sequentially process every registered export action for the current card.
                foreach (KeyValuePair<string, MTGExportAction> exportAction in ms_ExportActions)
                {
                    exportAction.Value.Invoke(cards[i], exportAction.Key, exportData);
                }

                // Append the card delimiter block, except for the very last card.
                if (i < numberOfCards - 1)
                {
                    exportData.Append(separator);
                }
            }

            await File.WriteAllTextAsync(filePath, exportData.ToString());
        }

        /// <summary>
        /// Parses a single raw text chunk representing an individual card and populates a new data model.
        /// </summary>
        /// <param name="cardEntry">The raw, multi-line string block containing the specific card's data.</param>
        /// <param name="cards">The running list of previously parsed cards, used to resolve parent inheritance references.</param>
        /// <returns>A new <see cref="MTGCreatureCard"/> populated with the parsed data.</returns>
        protected static MTGCreatureCard CreateMTGCreatureCard(string cardEntry, List<MTGCreatureCard> cards)
        {
            MTGCreatureCard card = new MTGCreatureCard();

            // Looks for a top level key (e.g., "mana:"), an optional inline value, and an optional indented multi-line block immediately following it.
            Regex regex = new Regex(@"^(?<key>\w+):(?:\s(?<value>[^\r\n]+))?\r?\n?(?<block>(?:^  .*?(?:\r?\n|$))*)", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(cardEntry);

            for (int i = 0; i < matches.Count; ++i)
            {
                string key = matches[i].Groups["key"].Value;

                // Look up the specific parser action for this top-level key.
                if (ms_ParseActions.TryGetValue(key, out Action<MTGCreatureCard, string, List<MTGCreatureCard>> parseAction))
                {
                    string value = matches[i].Groups["value"].Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Standard inline value assignment (e.g., "card: Goblin").
                        parseAction.Invoke(card, value, cards);
                        continue;
                    }

                    string block = matches[i].Groups["block"].Value.TrimStart();
                    if (!string.IsNullOrEmpty(block))
                    {
                        // Multi-line block parsing (e.g., indented lines under "mana:").
                        parseAction.Invoke(card, block, cards);
                    }
                }
            }

            return card;
        }

        /// <summary>
        /// Applies the parsed card name string directly to the model.
        /// </summary>
        /// <param name="card">The card data model being populated.</param>
        /// <param name="data">The raw string value extracted for this property.</param>
        /// <param name="cards">The list of previously parsed cards (unused in this action).</param>
        protected static void CardName(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            card.Name = data;
        }

        /// <summary>
        /// Resolves inheritance by scanning the list of previously imported cards for a matching name 
        /// and linking it as this card's parent.
        /// </summary>
        /// <param name="card">The card data model being populated.</param>
        /// <param name="data">The raw string value extracted for this property (expected to be the exact name of a parent card).</param>
        /// <param name="cards">The list of previously parsed cards to search within.</param>
        protected static void CardInherits(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            // A child card can only inherit from a parent that appeared BEFORE it in the text file.
            foreach (MTGCreatureCard creatureCard in cards)
            {
                if (creatureCard.Name == data)
                {
                    card.ParentCreatureCard = creatureCard;
                    return;
                }
            }

            Debug.WriteLine($"Creature {card.Name} inherits from {data} but can't find base creature card.");
        }

        /// <summary>
        /// Applies the card category from the text value.
        /// </summary>
        protected static void CardCategory(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            if (Enum.TryParse(data.Replace(" ", ""), ignoreCase: true, out CategoryType category))
            {
                card.Category = category;
                return;
            }

            Debug.WriteLine($"Creature {card.Name} has an unknown category: {data}.");
        }

        /// <summary>
        /// Converts the raw string category into the strongly-typed <see cref="CategoryType"/> enum and applies it to the card.
        /// </summary>
        /// <param name="card">The card data model being populated.</param>
        /// <param name="data">The raw string value extracted for this property.</param>
        /// <param name="cards">The list of previously parsed cards (unused in this action).</param>
        protected static void CardMana(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> manaColors = GetBlockInformation(data);

            card.Mana[ManaType.Generic] = manaColors.GetInt("generic");
            card.Mana[ManaType.White] = manaColors.GetInt("white");
            card.Mana[ManaType.Blue] = manaColors.GetInt("blue");
            card.Mana[ManaType.Black] = manaColors.GetInt("black");
            card.Mana[ManaType.Red] = manaColors.GetInt("red");
            card.Mana[ManaType.Green] = manaColors.GetInt("green");
        }

        /// <summary>
        /// Parses the multi-line block of combat statistics and applies the local power and toughness values to the card.
        /// </summary>
        /// <param name="card">The card data model being populated.</param>
        /// <param name="data">The indented block of text containing stat keys and values.</param>
        /// <param name="cards">The list of previously parsed cards (unused in this action).</param>
        protected static void CardStats(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> stats = GetBlockInformation(data);

            card.Power = stats.GetInt("power");
            card.Toughness = stats.GetInt("toughness");
        }

        /// <summary>
        /// Parses the multi-line block of traits and applies arrays of tags and keywords to the card.
        /// </summary>
        /// <param name="card">The card data model being populated.</param>
        /// <param name="data">The indented block of text containing trait keys and string arrays.</param>
        /// <param name="cards">The list of previously parsed cards (unused in this action).</param>
        protected static void CardTraits(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> traits = GetBlockInformation(data);

            card.Tags = traits.GetStringArray("tags").ToList();
            card.Keywords = traits.GetStringArray("keywords").ToList();
        }

        /// <summary>
        /// Parses the multi-line block of text descriptions and explicit override toggles to apply to the card.
        /// </summary>
        /// <param name="card">The card data model being populated.</param>
        /// <param name="data">The indented block of text containing rule text and flavor text settings.</param>
        /// <param name="cards">The list of previously parsed cards (unused in this action).</param>
        protected static void CardText(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> texts = GetBlockInformation(data);

            card.OverridesDescription = texts.GetBool("overrides_description", true);
            card.Description = texts.GetString("description");

            card.OverridesFlavorText = texts.GetBool("overrides_flavor_text", true);
            card.FlavorText = texts.GetString("flavor_text");
        }

        /// <summary>
        /// Helper method that decomposes an indented multi-line text block into a dictionary of localized key/value string pairs.
        /// </summary>
        /// <param name="data">The raw, multi-line string block to process.</param>
        /// <returns>A dictionary containing the parsed sub-keys and their corresponding values.</returns>
        protected static Dictionary<string, string> GetBlockInformation(string data)
        {
            Dictionary<string, string> blockInformation = new Dictionary<string, string>();

            string[] blockData = data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string block in blockData)
            {
                // Each line within a block is expected to be a sub-key mapping (e.g., "power: 3")
                string[] keyAndValue = block.Trim().Split(':', 2);
                if (keyAndValue.Length == 2)
                {
                    blockInformation[keyAndValue[0].Trim()] = keyAndValue[1].Trim();
                }
            }

            return blockInformation;
        }

        /// <summary>
        /// Serializes the card's name into the output buffer.
        /// </summary>
        /// <param name="card">The card being exported.</param>
        /// <param name="exportKey">The text key ("card").</param>
        /// <param name="output">The string builder instance constructing the file.</param>
        protected static void ExportCardName(MTGCreatureCard card, string exportKey, StringBuilder output)
        {
            output.Append(exportKey).Append(": ").Append(card.Name).Append(Environment.NewLine);
        }

        /// <summary>
        /// Serializes the inheritance link if the card depends on a parent card.
        /// </summary>
        /// <param name="card">The card being exported.</param>
        /// <param name="exportKey">The text key ("inherits").</param>
        /// <param name="output">The string builder instance constructing the file.</param>
        protected static void ExportParentCard(MTGCreatureCard card, string exportKey, StringBuilder output)
        {
            if (card.ParentCreatureCard != null)
            {
                output.Append(exportKey).Append(": ").Append(card.ParentCreatureCard.Name).Append(Environment.NewLine);
            }
        }

        /// <summary>
        /// Serializes the card category, bypassing the export entirely if the category perfectly matches the inherited parent's category.
        /// </summary>
        /// <param name="card">The card being exported.</param>
        /// <param name="exportKey">The text key ("category").</param>
        /// <param name="output">The string builder instance constructing the file.</param>
        protected static void ExportCategory(MTGCreatureCard card, string exportKey, StringBuilder output)
        {
            // Only export the category if it explicitly overrides the parent, or if there is no parent.
            if (card.Category != CategoryType.None && (card.ParentCreatureCard == null || card.Category != card.ParentCreatureCard.Category))
            {
                string categoryAsString = card.Category.ToString();

                Type enumType = card.Category.GetType();
                FieldInfo? fieldInfo = enumType.GetField(categoryAsString);

                if (fieldInfo != null)
                {
                    // Extract the human-readable DisplayAttribute to ensure the text file contains the friendly name
                    // (e.g., "Artifact Creature") rather than the code enum name.
                    DisplayAttribute? attribute = fieldInfo.GetCustomAttribute<DisplayAttribute>();
                    if (attribute != null && attribute.Name != null)
                    {
                        categoryAsString = attribute.Name;
                    }
                }

                output.Append(exportKey).Append(": ").Append(categoryAsString).Append(Environment.NewLine);
            }
        }

        /// <summary>
        /// Serializes the card's non-zero mana costs into an indented multi-line block.
        /// </summary>
        /// <param name="card">The card being exported.</param>
        /// <param name="exportKey">The text key ("mana").</param>
        /// <param name="output">The string builder instance constructing the file.</param>
        protected static void ExportMana(MTGCreatureCard card, string exportKey, StringBuilder output)
        {
            // Cache the buffer length before appending the header so we can revert it if the block is completely empty.
            int blockInformationStart = output.Length;

            bool hasMana = false;

            output.Append(exportKey).Append(':');

            foreach (KeyValuePair<ManaType, int> mana in card.Mana)
            {
                if (mana.Value > 0)
                {
                    output.Append(Environment.NewLine).Append("  ").Append(mana.Key.ToString().ToLower()).Append(": ").Append(mana.Value);

                    hasMana = true;
                }
            }

            if (hasMana)
            {
                output.Append(Environment.NewLine);
            }
            else
            {
                // If no local mana costs exist, slice the header off the buffer entirely.
                output.Length = blockInformationStart;
            }
        }

        /// <summary>
        /// Serializes the card's combat statistics (power and toughness) into an indented multi-line block.
        /// </summary>
        /// <param name="card">The card being exported.</param>
        /// <param name="exportKey">The text key ("stats").</param>
        /// <param name="output">The string builder instance constructing the file.</param>
        protected static void ExportStats(MTGCreatureCard card, string exportKey, StringBuilder output)
        {
            // Cache the buffer length before appending the header so we can revert it if the block is completely empty.
            int blockInformationStart = output.Length;

            bool hasStats = false;

            output.Append(exportKey).Append(':');

            if (card.Power > 0)
            {
                output.Append(Environment.NewLine).Append("  power: ").Append(card.Power);

                hasStats = true;
            }

            if (card.Toughness > 0)
            {
                output.Append(Environment.NewLine).Append("  toughness: ").Append(card.Toughness);

                hasStats = true;
            }

            if (hasStats)
            {
                output.Append(Environment.NewLine);
            }
            else
            {
                // If no local stats are to be exported, slice the header off the buffer entirely.
                output.Length = blockInformationStart;
            }
        }

        /// <summary>
        /// Serializes the arrays of tags and keywords into a comma-separated format within a multi-line block.
        /// </summary>
        /// <param name="card">The card being exported.</param>
        /// <param name="exportKey">The text key ("traits").</param>
        /// <param name="output">The string builder instance constructing the file.</param>
        protected static void ExportTraits(MTGCreatureCard card, string exportKey, StringBuilder output)
        {
            // Cache the buffer length before appending the header so we can revert it if the block is completely empty.
            int blockInformationStart = output.Length;

            bool hasTraits = false;

            output.Append(exportKey).Append(':');

            if (card.Tags.Count > 0)
            {
                output.Append(Environment.NewLine).Append("  tags: ");

                int numberOfTags = card.Tags.Count;
                for (int i = 0; i < numberOfTags; ++i)
                {
                    output.Append(card.Tags[i]);

                    if (i < numberOfTags - 1)
                    {
                        output.Append(", ");
                    }
                }

                hasTraits = true;
            }

            if (card.Keywords.Count > 0)
            {
                output.Append(Environment.NewLine).Append("  keywords: ");

                int numberOfKeywords = card.Keywords.Count;
                for (int i = 0; i < numberOfKeywords; ++i)
                {
                    output.Append(card.Keywords[i]);

                    if (i < numberOfKeywords - 1)
                    {
                        output.Append(", ");
                    }
                }

                hasTraits = true;
            }

            if (hasTraits)
            {
                output.Append(Environment.NewLine);
            }
            else
            {
                // If no local traits are to be exported, slice the header off the buffer entirely.
                output.Length = blockInformationStart;
            }
        }

        /// <summary>
        /// Serializes the description text, flavor text, and their explicit inheritance override toggles.
        /// </summary>
        /// <param name="card">The card being exported.</param>
        /// <param name="exportKey">The text key ("text").</param>
        /// <param name="output">The string builder instance constructing the file.</param>
        protected static void ExportText(MTGCreatureCard card, string exportKey, StringBuilder output)
        {
            output.Append(exportKey).Append(':').Append(Environment.NewLine);

            if (card.OverridesDescription)
            {
                output.Append("  description: ").Append(card.Description);
            }
            else
            {
                // Explicitly record that this card defers to its parent's rules text.
                output.Append("  overrides_description: False");
            }

            output.Append(Environment.NewLine);

            if (card.OverridesFlavorText)
            {
                output.Append("  flavor_text: ").Append(card.FlavorText);
            }
            else
            {
                // Explicitly record that this card defers to its parent's flavor text.
                output.Append("  overrides_flavor_text: False");
            }
        }
    }
}

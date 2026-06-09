using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

using MTGCreateYourOwnCreature.Model.Cards;
using MTGCreateYourOwnCreature.Model.Category;
using MTGCreateYourOwnCreature.ViewModel.Helpers;

namespace MTGCreateYourOwnCreature.Model.Parsing
{
    /// <summary>
    /// Provides functionality to read and parse creature cards from the custom text format used by the editor.
    /// Converts unstructured string data into populated <see cref="MTGCreatureCard"/> data models.
    /// </summary>
    public class MTGCreaturesParser
    {
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

            card.Mana[Mana.ManaType.Generic] = manaColors.GetInt("generic");
            card.Mana[Mana.ManaType.White] = manaColors.GetInt("white");
            card.Mana[Mana.ManaType.Blue] = manaColors.GetInt("blue");
            card.Mana[Mana.ManaType.Black] = manaColors.GetInt("black");
            card.Mana[Mana.ManaType.Red] = manaColors.GetInt("red");
            card.Mana[Mana.ManaType.Green] = manaColors.GetInt("green");
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
    }
}

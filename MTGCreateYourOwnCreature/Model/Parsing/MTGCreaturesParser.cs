using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Category;
using MTGCreateYourOwnCreature.ViewModel.Helpers;

namespace MTGCreateYourOwnCreature.Model.Parsing
{
    /// <summary>
    /// Reads creature cards from the custom text format used by the editor.
    /// </summary>
    public class MTGCreaturesParser
    {
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
        /// Imports every creature card found in the selected text file.
        /// </summary>
        public static List<MTGCreatureCard> Parse(string filePath)
        {
            List<MTGCreatureCard> cards = new List<MTGCreatureCard>();

            string fileText = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(fileText))
            {
                return cards;
            }

            string[] cardsText = Regex.Split(fileText.Trim(), @"\r?\n\r?\n---\r?\n\r?\n", RegexOptions.Multiline);
            for (int i = 0; i < cardsText.Length; ++i)
            {
                if (string.IsNullOrWhiteSpace(cardsText[i]))
                {
                    continue;
                }

                cards.Add(CreateMTGCreatureCard(cardsText[i], cards));
            }

            return cards;
        }

        /// <summary>
        /// Builds one card from its text block.
        /// </summary>
        protected static MTGCreatureCard CreateMTGCreatureCard(string cardEntry, List<MTGCreatureCard> cards)
        {
            MTGCreatureCard card = new MTGCreatureCard();

            Regex regex = new Regex(@"^(?<key>\w+):(?:\s(?<value>[^\r\n]+))?\r?\n?(?<block>(?:^  .*?(?:\r?\n|$))*)", RegexOptions.Multiline);
            MatchCollection matches = regex.Matches(cardEntry);

            for (int i = 0; i < matches.Count; ++i)
            {
                string key = matches[i].Groups["key"].Value;

                if (ms_ParseActions.TryGetValue(key, out Action<MTGCreatureCard, string, List<MTGCreatureCard>> parseAction))
                {
                    string value = matches[i].Groups["value"].Value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        parseAction.Invoke(card, value, cards);
                        continue;
                    }

                    string block = matches[i].Groups["block"].Value.TrimStart();
                    if (!string.IsNullOrEmpty(block))
                    {
                        parseAction.Invoke(card, block, cards);
                    }
                }
            }

            return card;
        }


        /// <summary>
        /// Applies the parsed card name.
        /// </summary>
        protected static void CardName(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            card.Name = data;
        }

        /// <summary>
        /// Links the card with a parent card that was imported before it.
        /// </summary>
        protected static void CardInherits(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
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
        /// Applies the local mana cost of the card.
        /// </summary>
        protected static void CardMana(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> manaColors = GetBlockInformation(data);

            card.Mana[Model.Mana.ManaType.Generic] = manaColors.GetInt("generic");
            card.Mana[Model.Mana.ManaType.White] = manaColors.GetInt("white");
            card.Mana[Model.Mana.ManaType.Blue] = manaColors.GetInt("blue");
            card.Mana[Model.Mana.ManaType.Black] = manaColors.GetInt("black");
            card.Mana[Model.Mana.ManaType.Red] = manaColors.GetInt("red");
            card.Mana[Model.Mana.ManaType.Green] = manaColors.GetInt("green");
        }

        /// <summary>
        /// Applies the local power and toughness of the card.
        /// </summary>
        protected static void CardStats(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> stats = GetBlockInformation(data);

            card.Power = stats.GetInt("power");
            card.Toughness = stats.GetInt("toughness");
        }

        /// <summary>
        /// Applies tags and keywords to the card.
        /// </summary>
        protected static void CardTraits(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> traits = GetBlockInformation(data);

            card.Tags = traits.GetStringArray("tags").ToList();
            card.Keywords = traits.GetStringArray("keywords").ToList();
        }

        /// <summary>
        /// Applies description and flavor text settings to the card.
        /// </summary>
        protected static void CardText(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> texts = GetBlockInformation(data);

            card.OverridesDescription = texts.GetBool("overrides_description", true);
            card.Description = texts.GetString("description");

            card.OverridesFlavorText = texts.GetBool("overrides_flavor_text", true);
            card.FlavorText = texts.GetString("flavor_text");
        }

        /// <summary>
        /// Converts an indented text block into key/value pairs.
        /// </summary>
        protected static Dictionary<string, string> GetBlockInformation(string data)
        {
            Dictionary<string, string> blockInformation = new Dictionary<string, string>();

            string[] blockData = data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string block in blockData)
            {
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

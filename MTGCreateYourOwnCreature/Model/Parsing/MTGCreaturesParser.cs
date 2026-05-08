using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

using MTGCreateYourOwnCreature.Model;

namespace MTGCreateYourOwnCreature.ViewModel.Helpers
{
    public class MTGCreaturesParser
    {
        protected static Dictionary<string, Action<MTGCreatureCard, string, List<MTGCreatureCard>>> ms_ParseActions = new Dictionary<string, Action<MTGCreatureCard, string, List<MTGCreatureCard>>>()
        {
            { "card", CardName },
            { "inherits", CardInherits },
            { "category", CardCategory },
            { "mana", CardMana },
            { "stats", CardStats },
            { "traits", CardTraits },
            { "text", CardText },
        };

        public static List<MTGCreatureCard> Parse(string filePath)
        {
            List<MTGCreatureCard> cards = new List<MTGCreatureCard>();

            string fileText = File.ReadAllText(filePath);

            string[] cardsText = fileText.Split(new string[] { "\r\n\r\n---\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < cardsText.Length; ++i)
            {
                cards.Add(CreateMTGCreatureCard(cardsText[i], cards));
            }

            return cards;
        }

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
                        parseAction?.Invoke(card, value, cards);
                        continue;
                    }

                    string block = matches[i].Groups["block"].Value.TrimStart();
                    if (!string.IsNullOrEmpty(block))
                    {
                        parseAction?.Invoke(card, block, cards);
                    }
                }
            }

            return card;
        }


        protected static void CardName(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            card.Name = data;
        }

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

        protected static void CardCategory(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            card.Category = data;
        }

        protected static void CardMana(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> manaColors = GetBlockInformation(data);

            card.Mana = new Dictionary<Model.Mana.ManaType, int>();

            card.Mana[Model.Mana.ManaType.Colorless] = manaColors.GetInt("colorless");
            card.Mana[Model.Mana.ManaType.White] = manaColors.GetInt("white");
            card.Mana[Model.Mana.ManaType.Blue] = manaColors.GetInt("blue");
            card.Mana[Model.Mana.ManaType.Black] = manaColors.GetInt("black");
            card.Mana[Model.Mana.ManaType.Red] = manaColors.GetInt("red");
            card.Mana[Model.Mana.ManaType.Green] = manaColors.GetInt("green");
        }

        protected static void CardStats(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> stats = GetBlockInformation(data);
            card.Stats = new MTGCreatureCard.MTGCreatureStats(stats.GetInt("power"), stats.GetInt("toughness"));
        }

        protected static void CardTraits(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> traits = GetBlockInformation(data);
            card.Traits = new MTGCreatureCard.MTGCreatureTraits(traits.GetStringArray("tags"), traits.GetStringArray("keywords"));
        }

        protected static void CardText(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, string> texts = GetBlockInformation(data);
            card.Description = texts.GetString("description");
            card.Lore = texts.GetString("lore");
        }

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

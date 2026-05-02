using MTGCreateYourOwnCreature.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MTGCreateYourOwnCreature.ViewModel.Helpers
{
    public class MTGCreaturesParser
    {
        protected static Dictionary<string, Action<MTGCreatureCard, string, List<MTGCreatureCard>>> ms_ParseActions = new Dictionary<string, Action<MTGCreatureCard, string, List<MTGCreatureCard>>>()
        {
            { "card", CardName },
            { "category", CardCategory },
            { "mana", CardMana },
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

        protected static void CardCategory(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            card.Category = data;
        }

        protected static void CardMana(MTGCreatureCard card, string data, List<MTGCreatureCard> cards)
        {
            Dictionary<string, int> manaColors = new Dictionary<string, int>();

            string[] dataManaColors = data.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string manaColor in dataManaColors)
            {
                string[] manaColorAndQuantity = manaColor.Trim().Split(':', 2);
                if (manaColorAndQuantity.Length == 2)
                {
                    manaColors[manaColorAndQuantity[0].Trim()] = int.Parse(manaColorAndQuantity[1].Trim());
                }
            }

            int GetMana(string manaColor) => manaColors.TryGetValue(key: manaColor, out int value) ? value : 0;

            card.Mana = new MTGCreatureCard.MTGCreatureMana(GetMana("colorless"), GetMana("white"), GetMana("blue"),
                GetMana("black"), GetMana("red"), GetMana("green"));
        }
    }
}

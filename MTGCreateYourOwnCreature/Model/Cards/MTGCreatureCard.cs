
using MTGCreateYourOwnCreature.Model.Category;
using MTGCreateYourOwnCreature.Model.Mana;
using System.Windows.Controls;

namespace MTGCreateYourOwnCreature.Model
{
    public class MTGCreatureCard
    {
        public string Name { get; set; }

        public MTGCreatureCard? ParentCreatureCard { get; set; }

        public string ParentCreatureCardDisplayName => ParentCreatureCard?.Name ?? "Base Creature";

        public CategoryType Category { get; set; }

        public Array AvailableCategories => Enum.GetValues<CategoryType>();

        public Dictionary<ManaType, int> Mana { get; set; }

        public Dictionary<ManaType, int> TotalMana { get => GetTotalMana(); }


        public class MTGCreatureStats
        {
            public int Power { get; set; }

            public int Toughness { get; set; }

            public MTGCreatureStats(int power, int toughness)
            {
                Power = power;
                Toughness = toughness;
            }
        }

        public MTGCreatureStats Stats { get; set; }

        public class MTGCreatureTraits
        {
            public List<string> Tags { get; set; }

            public List<string> Keywords { get; set; }

            public MTGCreatureTraits(string[] tags, string[] keywords)
            {
                Tags = tags.ToList();
                Keywords = keywords.ToList();
            }
        }

        public MTGCreatureTraits Traits { get; set; }

        public string Description { get; set; }

        public string Lore { get; set; }


        public MTGCreatureCard()
        {
            Name = string.Empty;
            ParentCreatureCard = null;
            Category = CategoryType.Creature;
            Mana = new Dictionary<ManaType, int>();
            Stats = new MTGCreatureStats(power: 0, toughness: 0);
            Traits = new MTGCreatureTraits([], []);
            Description = string.Empty;
            Lore = string.Empty;
        }


        protected Dictionary<ManaType, int> GetTotalMana()
        {
            Dictionary<ManaType, int> mana = new Dictionary<ManaType, int>(Mana);

            if (ParentCreatureCard != null)
            {
                Dictionary<ManaType, int> parentMana = ParentCreatureCard.TotalMana;
                foreach (KeyValuePair<ManaType, int> manaPair in parentMana)
                {
                    mana[manaPair.Key] += manaPair.Value;
                }
            }

            return mana;
        }
    }
}

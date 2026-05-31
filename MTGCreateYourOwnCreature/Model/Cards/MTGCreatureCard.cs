
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Model.Category;

namespace MTGCreateYourOwnCreature.Model
{
    public class MTGCreatureCard
    {
        public string Name { get; set; }

        public MTGCreatureCard? ParentCreatureCard { get; set; }

        public CategoryType Category { get; set; }

        public Dictionary<ManaType, int> Mana { get; set; }

        public int Power { get; set; }

        public int Toughness { get; set; }

        public List<string> Tags { get; set; }

        public List<string> Keywords { get; set; }

        public bool OverridesDescription { get; set; }

        public string Description { get; set; }

        public bool OverridesFlavorText { get; set; }

        public string FlavorText { get; set; }


        public MTGCreatureCard()
        {
            Name = string.Empty;
            ParentCreatureCard = null;
            Category = CategoryType.None;
            Mana = new Dictionary<ManaType, int>();
            Power = 0;
            Toughness = 0;
            Tags = new List<string>();
            Keywords = new List<string>();
            OverridesDescription = false;
            Description = string.Empty;
            OverridesFlavorText = false;
            FlavorText = string.Empty;
        }

        public static MTGCreatureCard CreateBaseCreatureCard()
        {
            MTGCreatureCard baseCreatureCard = new MTGCreatureCard();

            baseCreatureCard.Name = "Base Creature";

            ManaType[] manaTypes = Enum.GetValues<ManaType>();
            foreach (ManaType manaType in manaTypes)
            {
                baseCreatureCard.Mana[manaType] = 0;
            }

            return baseCreatureCard;
        }
    }
}

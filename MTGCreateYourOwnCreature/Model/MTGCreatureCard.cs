
namespace MTGCreateYourOwnCreature.Model
{
    public class MTGCreatureCard
    {
        public string Name { get; set; }

        public MTGCreatureCard? ParentCreatureCard { get; set; }

        public string Category { get; set; }


        public class MTGCreatureMana
        {
            public int Colorless { get; set; }

            public int White { get; set; }

            public int Blue { get; set; }

            public int Black { get; set; }

            public int Red { get; set; }

            public int Green { get; set; }


            public MTGCreatureMana(int colorless, int white, int blue, int black, int red, int green)
            {
                Colorless = colorless;
                White = white;
                Blue = blue;
                Black = black;
                Red = red;
                Green = green;
            }
        }

        public MTGCreatureMana Mana { get; set; }


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
            Category = string.Empty;
            Mana = new MTGCreatureMana(colorless: 0, white: 0, blue: 0, black: 0, red: 0, green: 0);
            Stats = new MTGCreatureStats(power: 0, toughness: 0);
            Traits = new MTGCreatureTraits([], []);
            Description = string.Empty;
            Lore = string.Empty;
        }
    }
}

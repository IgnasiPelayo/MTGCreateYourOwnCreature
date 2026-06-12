
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Model.Category;

namespace MTGCreateYourOwnCreature.Model.Cards
{
    /// <summary>
    /// Stores the editable data of a single creature card.
    /// Acts as the core data model, supporting an inheritance system via ParentCreatureCard.
    /// </summary>
    public class MTGCreatureCard
    {
        /// <summary>
        /// The name of the creature card.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The template card this creature inherits its base properties from.
        /// Set to null if this is a root/base card.
        /// </summary>
        public MTGCreatureCard? ParentCreatureCard { get; set; }

        /// <summary>
        /// The category type of the creature.
        /// </summary>
        public CategoryType Category { get; set; }

        /// <summary>
        /// Maps every available ManaType to its required integer cost.
        /// Guaranteed to contain all enum values initialized to 0 upon instantiation.
        /// </summary>
        public Dictionary<ManaType, int> Mana { get; set; }

        /// <summary>
        /// The power value of the creature.
        /// </summary>
        public int Power { get; set; }

        /// <summary>
        /// The toughness value of the creature.
        /// </summary>
        public int Toughness { get; set; }

        /// <summary>
        /// The list of tags associated with the creature.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// The list of gameplay keywords associated with the creature.
        /// </summary>
        public List<string> Keywords { get; set; }

        /// <summary>
        /// Flags whether it has explicitly overridden the inherited Description.
        /// </summary>
        public bool OverridesDescription { get; set; }

        /// <summary>
        /// Describes the rules text description of the creature.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Flags whether it has explicitly overridden the inherited FlavorText.
        /// </summary>
        public bool OverridesFlavorText { get; set; }

        /// <summary>
        /// Displays the lore or flavor text of the creature.
        /// </summary>
        public string FlavorText { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MTGCreatureCard"/> class with default values.
        /// </summary>
        public MTGCreatureCard()
        {
            Name = string.Empty;
            ParentCreatureCard = null;

            Category = CategoryType.None;

            Mana = CreateEmptyManaCost();

            Power = 0;
            Toughness = 0;

            Tags = new List<string>();
            Keywords = new List<string>();

            OverridesDescription = false;
            Description = string.Empty;
            OverridesFlavorText = false;
            FlavorText = string.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MTGCreatureCard"/> class by performing a deep copy of another card.
        /// </summary>
        /// <param name="source">The original card to duplicate.</param>
        public MTGCreatureCard(MTGCreatureCard source)
        {
            Name = $"{source.Name} (Copy)";
            ParentCreatureCard = source.ParentCreatureCard;

            Category = source.Category;

            Mana = new Dictionary<ManaType, int>(source.Mana);

            Power = source.Power;
            Toughness = source.Toughness;

            Tags = new List<string>(source.Tags);
            Keywords = new List<string>(source.Keywords);

            OverridesDescription = source.OverridesDescription;
            Description = source.Description;
            OverridesFlavorText = source.OverridesFlavorText;
            FlavorText = source.FlavorText;
        }

        /// <summary>
        /// Generates a dictionary pre-populated with every defined ManaType.
        /// </summary>
        /// <returns>A dictionary mapping all ManaType enum values to an initial cost of 0.</returns>
        protected static Dictionary<ManaType, int> CreateEmptyManaCost()
        {
            Dictionary<ManaType, int> mana = new Dictionary<ManaType, int>();

            ManaType[] manaTypes = Enum.GetValues<ManaType>();
            foreach (ManaType manaType in manaTypes)
            {
                mana[manaType] = 0;
            }

            return mana;
        }
    }
}

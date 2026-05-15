
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Model.Category;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGCreatureCardVM
    {
        public MTGCreatureCard Card { get; set; }

        public ObservableCollection<MTGManaEntryVM> Mana { get; set; }


        public MTGCreatureCardVM(MTGCreatureCard card)
        {
            Card = card;

            Mana = new ObservableCollection<MTGManaEntryVM>();
            foreach (KeyValuePair<ManaType, int> manaCost in Card.Mana)
            {
                Mana.Add(new MTGManaEntryVM(manaCost.Key, manaCost.Value));
            }
        }


        public String ResolvedParentCardName => Card.ParentCreatureCard?.Name ?? "Base Creature";

        public CategoryType ResolvedCategory
        {
            get
            {
                MTGCreatureCard? currentCard = Card;
                while (currentCard != null)
                {
                    if (currentCard.Category != CategoryType.None)
                    {
                        return currentCard.Category;
                    }

                    currentCard = currentCard.ParentCreatureCard;
                }

                return CategoryType.None;
            }

            set => Card.Category = value;
        }


        public List<MTGManaEntryVM> ResolvedTotalMana
        {
            get
            {
                Dictionary<ManaType, int> rawMana = new Dictionary<ManaType, int>();

                MTGCreatureCard? currentCard = Card;
                while (currentCard != null)
                {
                    foreach (KeyValuePair<ManaType, int> manaCost in currentCard.Mana)
                    {
                        rawMana[manaCost.Key] = rawMana.GetValueOrDefault(manaCost.Key) + manaCost.Value;
                    }

                    currentCard = currentCard.ParentCreatureCard;
                }

                List<MTGManaEntryVM> mana = new List<MTGManaEntryVM>();
                foreach (KeyValuePair<ManaType, int> manaCost in rawMana)
                {
                    mana.Add(new MTGManaEntryVM(manaCost.Key, manaCost.Value));
                }

                return mana;
            }
        }


        public Array AvailableCategories
        {
            get
            {
                CategoryType[] categories = Enum.GetValues<CategoryType>();
                List<CategoryType> availableCategories = new List<CategoryType>();

                foreach (CategoryType category in categories)
                {
                    if (category != CategoryType.None)
                    {
                        availableCategories.Add(category);
                    }
                }

                return availableCategories.ToArray();
            }
        }
    }
}

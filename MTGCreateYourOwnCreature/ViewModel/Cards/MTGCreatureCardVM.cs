using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Model.Category;
using System.ComponentModel;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGCreatureCardVM : INotifyPropertyChanged
    {
        public MTGCreatureCard Card { get; set; }

        public ObservableCollection<MTGManaEntryVM> Mana { get; set; }


        public MTGCreatureCardVM(MTGCreatureCard card)
        {
            Card = card;

            Mana = new ObservableCollection<MTGManaEntryVM>();
            ManaType[] manaTypes = Enum.GetValues<ManaType>();
            foreach (ManaType manaType in manaTypes)
            {
                Mana.Add(new MTGManaEntryVM(this, manaType));
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


        public IReadOnlyDictionary<ManaType, int> ResolvedTotalMana => GetTotalManaFromCard(Card);

        public IReadOnlyDictionary<ManaType, int> ResolvedInheritedMana => GetTotalManaFromCard(Card.ParentCreatureCard);

        public event PropertyChangedEventHandler? PropertyChanged;

        public IReadOnlyDictionary<ManaType, int> GetTotalManaFromCard(MTGCreatureCard? card)
        {
            Dictionary<ManaType, int> mana = new Dictionary<ManaType, int>();

            ManaType[] manaTypes = Enum.GetValues<ManaType>();
            foreach (ManaType manaType in manaTypes)
            {
                mana[manaType] = 0;
            }

            MTGCreatureCard? currentCard = card;
            while (currentCard != null)
            {
                foreach (KeyValuePair<ManaType, int> manaCost in currentCard.Mana)
                {
                    mana[manaCost.Key] += manaCost.Value;
                }

                currentCard = currentCard.ParentCreatureCard;
            }

            return mana;
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

        public void NotifyManaChanged(ManaType type)
        {
            OnPropertyChanged(nameof(ResolvedTotalMana));
            OnPropertyChanged(nameof(ResolvedInheritedMana));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

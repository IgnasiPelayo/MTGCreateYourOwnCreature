using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Model.Category;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

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
                MTGManaEntryVM manaEntry = new MTGManaEntryVM(manaType, Card.Mana[manaType], ResolvedInheritedMana[manaType]);
                manaEntry.PropertyChanged += OnManaEntryChanged;

                Mana.Add(manaEntry);
            }
        }


        public bool HasParentCard => Card.ParentCreatureCard != null;

        public string ParentCardName => Card.ParentCreatureCard?.Name ?? "";

        public string ResolvedParentCardName => Card.ParentCreatureCard?.Name ?? "Base Creature";

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


        public int Power
        {
            get => Card.Power;
            set
            {
                if (Card.Power == value)
                {
                    return;
                }

                Card.Power = value;

                OnPropertyChanged(nameof(Power));
                OnPropertyChanged(nameof(ResolvedTotalPower));
            }
        }

        public int ResolvedTotalPower => Card.Power + ResolvedInheritedPower;

        public int ResolvedInheritedPower => Card.ParentCreatureCard?.Power ?? 0;


        public int Toughness
        {
            get => Card.Toughness;
            set
            {
                if (Card.Toughness == value)
                {
                    return;
                }

                Card.Toughness = value;

                OnPropertyChanged(nameof(Toughness));
                OnPropertyChanged(nameof(ResolvedTotalToughness));
            }
        }

        public int ResolvedTotalToughness => Card.Toughness + ResolvedInheritedToughness;

        public int ResolvedInheritedToughness => Card.ParentCreatureCard?.Toughness ?? 0;


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

        public void Refresh()
        {
            OnPropertyChanged(nameof(ResolvedTotalMana));

            foreach (MTGManaEntryVM entry in Mana)
            {
                entry.InheritedValue = ResolvedInheritedMana[entry.Type];
            }

            OnPropertyChanged(nameof(ResolvedInheritedMana));
        }


        public void ChangeParent(MTGCreatureCard parent)
        {
            Card.ParentCreatureCard = parent;

            OnPropertyChanged(nameof(HasParentCard));
            OnPropertyChanged(nameof(ParentCardName));
            OnPropertyChanged(nameof(ResolvedParentCardName));

            OnPropertyChanged(nameof(ResolvedCategory));
            
            Refresh();

            OnPropertyChanged(nameof(Power));
            OnPropertyChanged(nameof(ResolvedTotalPower));
            OnPropertyChanged(nameof(ResolvedInheritedPower));

            OnPropertyChanged(nameof(Toughness));
            OnPropertyChanged(nameof(ResolvedTotalToughness));
            OnPropertyChanged(nameof(ResolvedInheritedToughness));
        }


        protected void OnManaEntryChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not MTGManaEntryVM entry)
            {
                return;
            }

            if (e.PropertyName != nameof(MTGManaEntryVM.Value))
            {
                return;
            }

            Card.Mana[entry.Type] = entry.Value;

            OnPropertyChanged(nameof(ResolvedTotalMana));
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Model.Category;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using MTGCreateYourOwnCreature.ViewModel.Commands;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGCreatureCardVM : INotifyPropertyChanged
    {
        public MTGCreatureCard Card { get; set; }

        public ObservableCollection<MTGManaEntryVM> Mana { get; set; }

        public ObservableCollection<MTGTraitEntryVM> Tags { get; set; }

        public string NewTag { get; set; }

        public ObservableCollection<MTGTraitEntryVM> Keywords { get; set; }

        public string NewKeyword { get; set; }


        protected readonly ICommand m_AddTagCommand;
        public ICommand AddTagCommand => m_AddTagCommand;

        protected readonly ICommand m_RemoveTagCommand;
        public ICommand RemoveTagCommand => m_RemoveTagCommand;

        protected readonly ICommand m_AddKeywordCommand;
        public ICommand AddKeywordCommand => m_AddKeywordCommand;

        protected readonly ICommand m_RemoveKeywordCommand;
        public ICommand RemoveKeywordCommand => m_RemoveKeywordCommand;


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

            Tags = new ObservableCollection<MTGTraitEntryVM>(GetTagsFromCard(Card));
            NewTag = string.Empty;

            Keywords = new ObservableCollection<MTGTraitEntryVM>(GetKeywordsFromCard(Card));
            NewKeyword = string.Empty;

            m_AddTagCommand = new RelayCommand(_ =>
            {
                if (OnTraitAdded(Tags, NewTag, nameof(Tags)))
                {
                    Card.Tags.Add(NewTag);

                    NewTag = string.Empty;
                    OnPropertyChanged(nameof(NewTag));
                }
            });

            m_RemoveTagCommand = new RelayCommand(param =>
            {
                if (param is not MTGTraitEntryVM trait)
                {
                    return;
                }

                Card.Tags.Remove(trait.Value);

                Tags.Remove(trait);
                OnPropertyChanged(nameof(Tags));
            });

            m_AddKeywordCommand = new RelayCommand(_ =>
            {
                if (OnTraitAdded(Keywords, NewKeyword, nameof(Keywords)))
                {
                    Card.Keywords.Add(NewKeyword);

                    NewKeyword = string.Empty;
                    OnPropertyChanged(nameof(NewKeyword));
                }
            });

            m_RemoveKeywordCommand = new RelayCommand(param =>
            {
                if (param is not MTGTraitEntryVM trait)
                {
                    return;
                }

                Card.Keywords.Remove(trait.Value);

                Keywords.Remove(trait);
                OnPropertyChanged(nameof(Keywords));
            });
        }


        public bool HasParentCard => Card.ParentCreatureCard != null;

        public string ParentCardName => Card.ParentCreatureCard?.Name ?? "";

        public string ResolvedParentCardName => Card.ParentCreatureCard?.Name ?? "Base Creature";

        public CategoryType ResolvedCategory
        {
            get => GetCategoryFromCard(Card);
            set
            {
                if (Card.Category == value)
                {
                    return;
                }

                Card.Category = value;

                OnPropertyChanged(nameof(ResolvedCategory));
                OnPropertyChanged(nameof(HasInheritedCategory));
            }
        }

        public bool HasInheritedCategory => HasParentCard && ResolvedCategory == GetCategoryFromCard(Card.ParentCreatureCard);

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


        protected CategoryType GetCategoryFromCard(MTGCreatureCard? card)
        {
            MTGCreatureCard? currentCard = card;
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


        protected IReadOnlyDictionary<ManaType, int> GetTotalManaFromCard(MTGCreatureCard? card)
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


        protected IReadOnlyCollection<MTGTraitEntryVM> GetTagsFromCard(MTGCreatureCard? card)
        {
            return GetTraitsFromCard(card, c => c.Tags);
        }

        protected IReadOnlyCollection<MTGTraitEntryVM> GetKeywordsFromCard(MTGCreatureCard? card)
        {
            return GetTraitsFromCard(card, c => c.Keywords);
        }

        protected IReadOnlyCollection<MTGTraitEntryVM> GetTraitsFromCard(MTGCreatureCard? card, Func<MTGCreatureCard, IEnumerable<string>> selector)
        {
            List<List<MTGTraitEntryVM>> cardsTraits = new List<List<MTGTraitEntryVM>>();

            MTGCreatureCard? currentCard = card;
            while (currentCard != null)
            {
                List<MTGTraitEntryVM> cardTraits = new List<MTGTraitEntryVM>();

                bool isInherited = currentCard != card;

                foreach (string trait in selector(currentCard))
                {
                    cardTraits.Add(new MTGTraitEntryVM(trait, isInherited));
                }

                cardsTraits.Add(cardTraits);

                currentCard = currentCard.ParentCreatureCard;
            }

            List<MTGTraitEntryVM> traits = new List<MTGTraitEntryVM>();

            for (int i = cardsTraits.Count - 1; i >= 0; --i)
            {
                traits.AddRange(cardsTraits[i]);
            }

            return traits;
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

        public void RecalculateMana()
        {
            OnPropertyChanged(nameof(ResolvedTotalMana));

            foreach (MTGManaEntryVM entry in Mana)
            {
                entry.InheritedValue = ResolvedInheritedMana[entry.Type];
            }

            OnPropertyChanged(nameof(ResolvedInheritedMana));
        }


        public void UpdateTags()
        {
            Tags.Clear();

            List<MTGTraitEntryVM> newTags = GetTagsFromCard(Card).ToList();
            foreach (MTGTraitEntryVM tag in newTags)
            {
                Tags.Add(tag);
            }

            OnPropertyChanged(nameof(Tags));
        }

        public void UpdateKeywords()
        {
            Keywords.Clear();

            List<MTGTraitEntryVM> newKeywords = GetKeywordsFromCard(Card).ToList();
            foreach (MTGTraitEntryVM keyword in newKeywords)
            {
                Keywords.Add(keyword);
            }

            OnPropertyChanged(nameof(Keywords));
        }


        public void ChangeParent(MTGCreatureCard parent)
        {
            Card.ParentCreatureCard = parent;

            OnPropertyChanged(nameof(HasParentCard));
            OnPropertyChanged(nameof(ParentCardName));
            OnPropertyChanged(nameof(ResolvedParentCardName));

            OnPropertyChanged(nameof(ResolvedCategory));
            OnPropertyChanged(nameof(HasInheritedCategory));

            RecalculateMana();

            OnPropertyChanged(nameof(Power));
            OnPropertyChanged(nameof(ResolvedTotalPower));
            OnPropertyChanged(nameof(ResolvedInheritedPower));

            OnPropertyChanged(nameof(Toughness));
            OnPropertyChanged(nameof(ResolvedTotalToughness));
            OnPropertyChanged(nameof(ResolvedInheritedToughness));

            UpdateTags();
            UpdateKeywords();
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


        protected bool OnTraitAdded(ObservableCollection<MTGTraitEntryVM> traits, string newValue, string traitsPropertyName)
        {
            string value = newValue.Trim();

            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (MTGTraitEntryVM trait in traits)
            {
                if (string.Equals(trait.Value, value, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            traits.Add(new MTGTraitEntryVM(value, false));

            OnPropertyChanged(traitsPropertyName);

            return true;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

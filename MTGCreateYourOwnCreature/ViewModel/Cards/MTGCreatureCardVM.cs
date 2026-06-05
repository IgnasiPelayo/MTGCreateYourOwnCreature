using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Model.Category;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using MTGCreateYourOwnCreature.ViewModel.Commands;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGCreatureCardVM : INotifyPropertyChanged
    {
        public MTGCreatureCard Card { get; set; }

        public ObservableCollection<MTGManaEntryVM> Mana { get; set; }

        public MTGTraitCollectionVM Tags { get; set; }

        public string ResolvedTags => GetResolvedTags(Tags);

        public MTGTraitCollectionVM Keywords { get; set; }

        public string ResolvedKeywords => GetResolvedKeywords(Keywords);

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
                OnPropertyChanged(nameof(IsLegendaryCreature));
            }
        }

        public bool HasInheritedCategory => HasParentCard && ResolvedCategory == GetCategoryFromCard(Card.ParentCreatureCard);

        public bool IsLegendaryCreature => IsLegendaryCategory(ResolvedCategory);

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
                OnPropertyChanged(nameof(ResolvedTotalPowerToughness));
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
                OnPropertyChanged(nameof(ResolvedTotalPowerToughness));
            }
        }

        public int ResolvedTotalToughness => Card.Toughness + ResolvedInheritedToughness;

        public int ResolvedInheritedToughness => Card.ParentCreatureCard?.Toughness ?? 0;


        public string ResolvedTotalPowerToughness => $"{ResolvedTotalPower}/{ResolvedTotalToughness}";


        public MTGInformationEntryVM Description { get; set; }

        public MTGInformationEntryVM FlavorText { get; set; }



        public Visibility PreviewSeparatorVisibility => (Description?.HasValue == true && FlavorText?.HasValue == true) ? Visibility.Visible : Visibility.Collapsed;

        protected Brush? m_PreviewBackgroundBrush;

        public Brush? PreviewBackgroundBrush => m_PreviewBackgroundBrush;

        protected Brush? m_PreviewBorderBrush;

        public Brush? PreviewBorderBrush => m_PreviewBorderBrush;

        protected Brush? m_PreviewFrameBrush;

        public Brush? PreviewFrameBrush => m_PreviewFrameBrush;

        protected Brush? m_PreviewInformationFrameBrush;

        public Brush? PreviewInformationFrameBrush => m_PreviewInformationFrameBrush;



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

            Tags = new MTGTraitCollectionVM(GetTagsFromCard(Card),
                canAdd: value =>
                {
                    foreach (string tag in Card.Tags)
                    {
                        if (string.Equals(tag, value, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }

                    return true;
                },
                onAdd: trait =>
                {
                    Card.Tags.Add(trait.Value);
                    OnPropertyChanged(nameof(Tags));
                    OnPropertyChanged(nameof(ResolvedTags));
                },
                onRemove: trait =>
                {
                    Card.Tags.Remove(trait.Value);
                    OnPropertyChanged(nameof(Tags));
                    OnPropertyChanged(nameof(ResolvedTags));
                });

            Keywords = new MTGTraitCollectionVM(GetKeywordsFromCard(Card),
                canAdd: value =>
                {
                    foreach (string keyword in Card.Keywords)
                    {
                        if (string.Equals(keyword, value, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }

                    return true;
                },
                onAdd: trait =>
                {
                    Card.Keywords.Add(trait.Value);
                    OnPropertyChanged(nameof(Keywords));
                    OnPropertyChanged(nameof(ResolvedKeywords));
                },
                onRemove: trait =>
                {
                    Card.Keywords.Remove(trait.Value);
                    OnPropertyChanged(nameof(Keywords));
                    OnPropertyChanged(nameof(ResolvedKeywords));
                });

            if (!Card.OverridesDescription)
            {
                Card.Description = Card.ParentCreatureCard?.Description ?? "";
            }
            Description = new MTGInformationEntryVM(Card.Description, Card.ParentCreatureCard?.Description ?? "", Card.OverridesDescription);
            Description.PropertyChanged += OnInformationPropertyChanged;

            if (!Card.OverridesFlavorText)
            {
                Card.FlavorText = Card.ParentCreatureCard?.FlavorText ?? "";
            }
            FlavorText = new MTGInformationEntryVM(Card.FlavorText, Card.ParentCreatureCard?.FlavorText ?? "", Card.OverridesFlavorText);
            Description.PropertyChanged += OnInformationPropertyChanged;

            UpdatePreviewBrushes();
        }


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

        protected static bool IsLegendaryCategory(CategoryType type)
        {
            switch (type)
            {
                case CategoryType.LegendaryCreature:
                case CategoryType.LegendaryArtifactCreature:
                case CategoryType.LegendaryEnchantmentCreature:
                    return true;

                default:
                    return false;
            }
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


        protected string GetResolvedTags(MTGTraitCollectionVM tags)
        {
            string resolvedTags = string.Empty;

            foreach (MTGTraitEntryVM tag in tags.Traits)
            {
                resolvedTags += tag.Value + " ";
            }

            if (!string.IsNullOrEmpty(resolvedTags))
            {
                resolvedTags = "— " + resolvedTags;
            }

            return resolvedTags.TrimEnd();
        }


        protected string GetResolvedKeywords(MTGTraitCollectionVM keywords)
        {
            string resolvedKeywords = string.Empty;

            foreach (MTGTraitEntryVM keyword in keywords.Traits)
            {
                string keywordValue = keyword.Value;

                if (!string.IsNullOrEmpty(resolvedKeywords))
                {
                    keywordValue = keywordValue.ToLower();
                }

                resolvedKeywords += keywordValue + ", ";
            }

            if (!string.IsNullOrEmpty(resolvedKeywords))
            {
                resolvedKeywords = resolvedKeywords.TrimEnd();
                resolvedKeywords = resolvedKeywords.Remove(resolvedKeywords.Length - 1, 1);

                resolvedKeywords += "\n\r";
            }

            return resolvedKeywords;
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
            UpdatePreviewBrushes();
        }


        public void UpdateTags()
        {
            Tags.Traits.Clear();

            List<MTGTraitEntryVM> newTags = GetTagsFromCard(Card).ToList();
            foreach (MTGTraitEntryVM tag in newTags)
            {
                Tags.Traits.Add(tag);
            }

            OnPropertyChanged(nameof(Tags));
            OnPropertyChanged(nameof(ResolvedTags));
        }

        public void UpdateKeywords()
        {
            Keywords.Traits.Clear();

            List<MTGTraitEntryVM> newKeywords = GetKeywordsFromCard(Card).ToList();
            foreach (MTGTraitEntryVM keyword in newKeywords)
            {
                Keywords.Traits.Add(keyword);
            }

            OnPropertyChanged(nameof(Keywords));
            OnPropertyChanged(nameof(ResolvedKeywords));
        }


        public void UpdateDescription()
        {
            Description.InheritedValue = Card.ParentCreatureCard?.Description ?? "";

            if (!Card.OverridesDescription)
            {
                Card.Description = Description.InheritedValue;
            }

            OnPropertyChanged(nameof(Description));
            Description.UpdateInheritance();

            OnPropertyChanged(nameof(PreviewSeparatorVisibility));
        }

        public void UpdateFlavorText()
        {
            FlavorText.InheritedValue = Card.ParentCreatureCard?.FlavorText ?? "";

            if (!Card.OverridesFlavorText)
            {
                Card.FlavorText = FlavorText.InheritedValue;
            }

            OnPropertyChanged(nameof(FlavorText));
            FlavorText.UpdateInheritance();

            OnPropertyChanged(nameof(PreviewSeparatorVisibility));
        }


        public void ChangeParent(MTGCreatureCard parent)
        {
            Card.ParentCreatureCard = parent;

            OnPropertyChanged(nameof(HasParentCard));
            OnPropertyChanged(nameof(ParentCardName));
            OnPropertyChanged(nameof(ResolvedParentCardName));

            OnPropertyChanged(nameof(ResolvedCategory));
            OnPropertyChanged(nameof(HasInheritedCategory));
            OnPropertyChanged(nameof(IsLegendaryCreature));

            RecalculateMana();

            OnPropertyChanged(nameof(Power));
            OnPropertyChanged(nameof(ResolvedTotalPower));
            OnPropertyChanged(nameof(ResolvedInheritedPower));

            OnPropertyChanged(nameof(Toughness));
            OnPropertyChanged(nameof(ResolvedTotalToughness));
            OnPropertyChanged(nameof(ResolvedInheritedToughness));

            UpdateTags();
            UpdateKeywords();

            UpdateDescription();
            UpdateFlavorText();
        }


        protected static Dictionary<ManaType, Brush?> PreviewBackgroundBrushes = new Dictionary<ManaType, Brush?>()
        {
            { ManaType.Generic, App.Current.TryFindResource("GenericBackgroundBrush") as Brush },
            { ManaType.White, App.Current.TryFindResource("WhiteBackgroundBrush") as Brush },
            { ManaType.Blue, App.Current.TryFindResource("BlueBackgroundBrush") as Brush },
            { ManaType.Black, App.Current.TryFindResource("BlackPreviewBackgroundBrush") as Brush },
            { ManaType.Red, App.Current.TryFindResource("RedBackgroundBrush") as Brush },
            { ManaType.Green, App.Current.TryFindResource("GreenBackgroundBrush") as Brush },
        };

        protected static Dictionary<ManaType, Brush?> PreviewBorderBrushes = new Dictionary<ManaType, Brush?>()
        {
            { ManaType.Generic, App.Current.TryFindResource("GenericPreviewBorderBrush") as Brush },
            { ManaType.White, App.Current.TryFindResource("WhitePreviewBorderBrush") as Brush },
            { ManaType.Blue, App.Current.TryFindResource("BluePreviewBorderBrush") as Brush },
            { ManaType.Black, App.Current.TryFindResource("BlackPreviewBorderBrush") as Brush },
            { ManaType.Red, App.Current.TryFindResource("RedPreviewBorderBrush") as Brush },
            { ManaType.Green, App.Current.TryFindResource("GreenPreviewBorderBrush") as Brush },
        };

        protected static Dictionary<ManaType, Brush?> PreviewFrameBrushes = new Dictionary<ManaType, Brush?>()
        {
            { ManaType.Generic, App.Current.TryFindResource("GenericPreviewFrameBrush") as Brush },
            { ManaType.White, App.Current.TryFindResource("WhitePreviewFrameBrush") as Brush },
            { ManaType.Blue, App.Current.TryFindResource("BluePreviewFrameBrush") as Brush },
            { ManaType.Black, App.Current.TryFindResource("BlackPreviewFrameBrush") as Brush },
            { ManaType.Red, App.Current.TryFindResource("RedPreviewFrameBrush") as Brush },
            { ManaType.Green, App.Current.TryFindResource("GreenPreviewFrameBrush") as Brush },
        };

        protected static Dictionary<ManaType, Brush?> PreviewInformationFrameBrushes = new Dictionary<ManaType, Brush?>()
        {
            { ManaType.Generic, App.Current.TryFindResource("GenericPreviewInformationFrameBrush") as Brush },
            { ManaType.White, App.Current.TryFindResource("WhitePreviewInformationFrameBrush") as Brush },
            { ManaType.Blue, App.Current.TryFindResource("BluePreviewInformationFrameBrush") as Brush },
            { ManaType.Black, App.Current.TryFindResource("BlackPreviewInformationFrameBrush") as Brush },
            { ManaType.Red, App.Current.TryFindResource("RedPreviewInformationFrameBrush") as Brush },
            { ManaType.Green, App.Current.TryFindResource("GreenPreviewInformationFrameBrush") as Brush },
        };

        protected static Brush MultiColorBackgroundBrush = App.Current.TryFindResource("MultiColorPreviewBackgroundBrush") as Brush;
        protected static Brush MultiColorBorderBrush = App.Current.TryFindResource("MultiColorPreviewBorderBrush") as Brush;
        protected static Brush MultiColorFrameBrush = App.Current.TryFindResource("MultiColorPreviewFrameBrush") as Brush;
        protected static Brush MultiColorInformationFrameBrush = App.Current.TryFindResource("MultiColorPreviewInformationFrameBrush") as Brush;

        protected void UpdatePreviewBrushes()
        {
            Dictionary<ManaType, bool> manaTypesInCard = new Dictionary<ManaType, bool>();

            foreach (KeyValuePair<ManaType, int> mana in ResolvedTotalMana)
            {
                if (mana.Value > 0)
                {
                    manaTypesInCard[mana.Key] = true;
                }
            }

            int manaTypesInCardCount = manaTypesInCard.Count;

            if (manaTypesInCardCount == 0 || (manaTypesInCardCount == 1 && manaTypesInCard.ContainsKey(ManaType.Generic)))
            {
                m_PreviewBackgroundBrush = PreviewBackgroundBrushes[ManaType.Generic];
                m_PreviewBorderBrush = PreviewBorderBrushes[ManaType.Generic];
                m_PreviewFrameBrush = PreviewFrameBrushes[ManaType.Generic];
                m_PreviewInformationFrameBrush = PreviewInformationFrameBrushes[ManaType.Generic];
            }
            else if (manaTypesInCardCount == 2 && manaTypesInCard.ContainsKey(ManaType.Generic) || manaTypesInCardCount == 1)
            {
                foreach (ManaType manaType in manaTypesInCard.Keys)
                {
                    if (manaType != ManaType.Generic)
                    {
                        m_PreviewBackgroundBrush = PreviewBackgroundBrushes[manaType];
                        m_PreviewBorderBrush = PreviewBorderBrushes[manaType];
                        m_PreviewFrameBrush = PreviewFrameBrushes[manaType];
                        m_PreviewInformationFrameBrush = PreviewInformationFrameBrushes[manaType];
                    }
                }
            }
            else
            {
                m_PreviewBackgroundBrush = MultiColorBackgroundBrush;
                m_PreviewBorderBrush = MultiColorBorderBrush;
                m_PreviewFrameBrush = MultiColorFrameBrush;
                m_PreviewInformationFrameBrush = MultiColorInformationFrameBrush;
            }

            OnPropertyChanged(nameof(PreviewBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBorderBrush));
            OnPropertyChanged(nameof(PreviewFrameBrush));
            OnPropertyChanged(nameof(PreviewInformationFrameBrush));
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
            UpdatePreviewBrushes();
        }


        protected void OnInformationPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not MTGInformationEntryVM entry)
            {
                return;
            }

            if (e.PropertyName != nameof(MTGInformationEntryVM.Value) && e.PropertyName != nameof(MTGInformationEntryVM.OverridesValue))
            {
                return;
            }

            if (entry == Description)
            {
                Card.Description = entry.ResolvedValue;
                Card.OverridesDescription = entry.OverridesValue;

                OnPropertyChanged(nameof(Description));
                OnPropertyChanged(nameof(PreviewSeparatorVisibility));
            }
            else if (entry == FlavorText)
            {
                Card.FlavorText = entry.ResolvedValue;
                Card.OverridesFlavorText = entry.OverridesValue;

                OnPropertyChanged(nameof(FlavorText));
                OnPropertyChanged(nameof(PreviewSeparatorVisibility));
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

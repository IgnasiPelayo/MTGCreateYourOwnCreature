
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Model.Cards;
using MTGCreateYourOwnCreature.Model.Category;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    /// <summary>
    /// The primary data context for the creature editor UI.
    /// Wraps a raw MTGCreatureCard model and handles the complex inheritance resolution 
    /// (e.g., cascading parent stats, merging tags, and calculating color identity).
    /// </summary>
    public class MTGCreatureCardVM : INotifyPropertyChanged
    {
        /// <summary>
        /// The underlying data model for the creature card.
        /// </summary>
        public MTGCreatureCard Card { get; set; }

        /// <summary>
        /// The given name of the creature card.
        /// </summary>
        public string Name
        {
            get => Card.Name;
            set
            {
                Card.Name = value;

                OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// The observable collection of mana entries used for UI binding.
        /// </summary>
        public ObservableCollection<MTGManaEntryVM> Mana { get; set; }

        /// <summary>
        /// The collection of tags associated with the creature.
        /// </summary>
        public MTGTraitCollectionVM Tags { get; set; }

        /// <summary>
        /// A pre-formatted string of all tags (base and inherited) for the XAML preview.
        /// Example output: "— Human Warrior"
        /// </summary>
        public string ResolvedTags => GetResolvedTags(Tags);

        /// <summary>
        /// The collection of gameplay keywords associated with the creature.
        /// </summary>
        public MTGTraitCollectionVM Keywords { get; set; }

        /// <summary>
        /// A pre-formatted string of all keywords (base and inherited) for the XAML preview.
        /// Example output: "Fear, haste, flying"
        /// </summary>
        public string ResolvedKeywords => GetResolvedKeywords(Keywords);

        /// <summary>
        /// Whether this card inherits from a parent card or not.
        /// </summary>
        public bool HasParentCard => Card.ParentCreatureCard != null;

        /// <summary>
        /// The resolved name of the parent card, falling back to "Base Creature" if none exists.
        /// </summary>
        public string ResolvedParentCardName => Card.ParentCreatureCard?.Name ?? "Base Creature";

        /// <summary>
        /// The resolved category type of the creature.
        /// </summary>
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

        /// <summary>
        /// Whether the current category is inherited from a parent card.
        /// </summary>
        public bool HasInheritedCategory => HasParentCard && ResolvedCategory == GetCategoryFromCard(Card.ParentCreatureCard);

        /// <summary>
        /// Whether the resolved category requires legendary UI styling.
        /// </summary>
        public bool IsLegendaryCreature => IsLegendaryCategory(ResolvedCategory);

        /// <summary>
        /// The total aggregated mana cost, combining base and inherited.
        /// </summary>
        public IReadOnlyDictionary<ManaType, int> ResolvedTotalMana => GetTotalManaFromCard(Card);

        /// <summary>
        /// The inherited mana cost from the parent card.
        /// </summary>
        public IReadOnlyDictionary<ManaType, int> ResolvedInheritedMana => GetTotalManaFromCard(Card.ParentCreatureCard);

        /// <summary>
        /// The base power value of the creature.
        /// </summary>
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

        /// <summary>
        /// The total resolved power, combining base and inherited.
        /// </summary>
        public int ResolvedTotalPower => Card.Power + ResolvedInheritedPower;

        /// <summary>
        /// The power value inherited from the parent card.
        /// </summary>
        public int ResolvedInheritedPower => Card.ParentCreatureCard?.Power ?? 0;

        /// <summary>
        /// The base toughness value of the creature.
        /// </summary>
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

        /// <summary>
        /// The total resolved toughness, combining base and inherited.
        /// </summary>
        public int ResolvedTotalToughness => Card.Toughness + ResolvedInheritedToughness;

        /// <summary>
        /// The toughness value inherited from the parent card.
        /// </summary>
        public int ResolvedInheritedToughness => Card.ParentCreatureCard?.Toughness ?? 0;

        /// <summary>
        /// A pre-formatted string for the XAML textblock combining Power and Toughness.
        /// Example output: "3/2"
        /// </summary>
        public string ResolvedTotalPowerToughness => $"{ResolvedTotalPower}/{ResolvedTotalToughness}";

        /// <summary>
        /// The ViewModel entry managing the rules description.
        /// </summary>
        public MTGInformationEntryVM Description { get; set; }

        /// <summary>
        /// The ViewModel entry managing the flavor text.
        /// </summary>2
        public MTGInformationEntryVM FlavorText { get; set; }

        /// <summary>
        /// Automatically manages the visibility of the horizontal line between Description and Flavor Text.
        /// Bound directly to the UI element's Visibility property.
        /// </summary>
        public Visibility PreviewSeparatorVisibility => (Description?.HasValue == true && FlavorText?.HasValue == true) ? Visibility.Visible : Visibility.Collapsed;

        /// <summary>
        /// The backing field for the preview background brush.
        /// </summary>
        protected Brush m_PreviewBackgroundBrush = Brushes.Pink;

        /// <summary>
        /// The brush used to render the card's background preview.
        /// </summary>
        public Brush PreviewBackgroundBrush => m_PreviewBackgroundBrush;

        /// <summary>
        /// The backing field for the preview border brush.
        /// </summary>
        protected Brush m_PreviewBorderBrush = Brushes.Pink;

        /// <summary>
        /// The brush used to render the card's outer border.
        /// </summary>
        public Brush PreviewBorderBrush => m_PreviewBorderBrush;

        /// <summary>
        /// The backing field for the preview frame brush.
        /// </summary>
        protected Brush m_PreviewFrameBrush = Brushes.Pink;

        /// <summary>
        /// The brush used to render the card's inner frame.
        /// </summary>
        public Brush PreviewFrameBrush => m_PreviewFrameBrush;

        /// <summary>
        /// The backing field for the preview information frame brush.
        /// </summary>
        protected Brush m_PreviewInformationFrameBrush = Brushes.Pink;

        /// <summary>
        /// The brush used to render the text information frames on the card.
        /// </summary>
        public Brush PreviewInformationFrameBrush => m_PreviewInformationFrameBrush;

        /// <summary>
        /// The formatted collector number string (e.g., "1/10") to be displayed at the bottom of the visual card preview.
        /// </summary>
        public string PreviewCollectorNumber { get; set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="MTGCreatureCardVM"/> class.
        /// </summary>
        /// <param name="card">The base creature card model to wrap.</param>
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
                    // Prevent duplicate tags (case-insensitive).
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
                    // Prevent duplicate keywords (case-insensitive).
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
            FlavorText.PropertyChanged += OnInformationPropertyChanged;

            UpdatePreviewBrushes();
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Resolves the first valid category type found in this card or its parent hierarchy.
        /// </summary>
        /// <param name="card">The card to evaluate.</param>
        /// <returns>The resolved <see cref="CategoryType"/>, or None if no category is found.</returns>
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

        /// <summary>
        /// Checks if the provided category type should trigger legendary creature styling.
        /// </summary>
        /// <param name="type">The category type to check.</param>
        /// <returns>True if the type is a legendary variant; otherwise, false.</returns>
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

        /// <summary>
        /// Aggregates this card's base mana cost with all inherited mana costs from parent cards.
        /// </summary>
        /// <param name="card">The starting card to evaluate.</param>
        /// <returns>A dictionary containing the total required mana cost.</returns>
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

        /// <summary>
        /// Gets all tags visible on this card, including inherited tags from parents.
        /// </summary>
        /// <param name="card">The card to evaluate.</param>
        /// <returns>A collection of tag traits formatted for the UI.</returns>
        protected IReadOnlyCollection<MTGTraitEntryVM> GetTagsFromCard(MTGCreatureCard? card)
        {
            return GetTraitsFromCard(card, c => c.Tags);
        }

        /// <summary>
        /// Gets all keywords visible on this card, including inherited keywords from parents.
        /// </summary>
        /// <param name="card">The card to evaluate.</param>
        /// <returns>A collection of keyword traits formatted for the UI.</returns>
        protected IReadOnlyCollection<MTGTraitEntryVM> GetKeywordsFromCard(MTGCreatureCard? card)
        {
            return GetTraitsFromCard(card, c => c.Keywords);
        }

        /// <summary>
        /// Collects traits from the full parent chain and reverses them into correct display order.
        /// </summary>
        /// <param name="card">The base card to evaluate.</param>
        /// <param name="selector">The function to select the desired trait strings.</param>
        /// <returns>A collection of UI-ready trait entries.</returns>
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

            // Iterate backwards so the oldest parent's traits appear first in the UI, followed by the specific derived card's traits last.
            for (int i = cardsTraits.Count - 1; i >= 0; --i)
            {
                traits.AddRange(cardsTraits[i]);
            }

            return traits;
        }

        /// <summary>
        /// Builds the formatted tag string used in the card preview.
        /// </summary>
        /// <param name="tags">The collection of tag traits.</param>
        /// <returns>The fully formatted tag string.</returns>
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

        /// <summary>
        /// Builds the formatted keyword string shown before the description text.
        /// </summary>
        /// <param name="keywords">The collection of keyword traits.</param>
        /// <returns>The fully formatted keyword string.</returns>
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

                resolvedKeywords += Environment.NewLine;
            }

            return resolvedKeywords;
        }

        /// <summary>
        /// Gets an array of valid categories for UI selection, excluding the None state.
        /// </summary>
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

        /// <summary>
        /// Refreshes mana totals and inherited mana after a parent or mana value changes.
        /// </summary>
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

        /// <summary>
        /// Refreshes visible tags after this card or a parent card changes.
        /// </summary>
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

        /// <summary>
        /// Refreshes visible keywords after this card or a parent card changes.
        /// </summary>
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

        /// <summary>
        /// Refreshes description inheritance status and validates preview separator visibility.
        /// </summary>
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

        /// <summary>
        /// Refreshes flavor text inheritance and preview visibility.
        /// </summary>
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

        /// <summary>
        /// Updates the collector number display text based on the card's current ordinal position within the entire loaded set.
        /// </summary>
        /// <param name="collectorNumber">The 0-based index of this specific card within the global collection.</param>
        /// <param name="total">The total number of creature cards currently loaded in the editor.</param>
        public void UpdateCollectorNumber(int collectorNumber, int total)
        {
            PreviewCollectorNumber = $"{collectorNumber + 1}/{total}";
            OnPropertyChanged(nameof(PreviewCollectorNumber));
        }

        /// <summary>
        /// Applies a new parent card and recalculates all inherited values.
        /// </summary>
        /// <param name="parent">The parent card to assign, or null to clear inheritance.</param>
        public void ChangeParent(MTGCreatureCard? parent)
        {
            Card.ParentCreatureCard = parent;

            OnPropertyChanged(nameof(HasParentCard));
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

            OnPropertyChanged(nameof(ResolvedTotalPowerToughness));

            UpdateTags();
            UpdateKeywords();

            UpdateDescription();
            UpdateFlavorText();
        }


        // Preview Brushes ----------------------------------------------------------------------------------------------------

        /// <summary>
        /// Pre-cached dictionary of background brushes matching their respective mana types.
        /// </summary>
        protected static readonly Dictionary<ManaType, Brush> PreviewBackgroundBrushes = new Dictionary<ManaType, Brush>()
        {
            { ManaType.Generic, FindBrush("GenericBackgroundBrush") },
            { ManaType.White, FindBrush("WhiteBackgroundBrush") },
            { ManaType.Blue, FindBrush("BlueBackgroundBrush") },
            { ManaType.Black, FindBrush("BlackPreviewBackgroundBrush") },
            { ManaType.Red, FindBrush("RedBackgroundBrush") },
            { ManaType.Green, FindBrush("GreenBackgroundBrush") },
        };

        /// <summary>
        /// Pre-cached dictionary of border brushes matching their respective mana types.
        /// </summary>
        protected static readonly Dictionary<ManaType, Brush> PreviewBorderBrushes = new Dictionary<ManaType, Brush>()
        {
            { ManaType.Generic, FindBrush("GenericPreviewBorderBrush") },
            { ManaType.White, FindBrush("WhitePreviewBorderBrush") },
            { ManaType.Blue, FindBrush("BluePreviewBorderBrush") },
            { ManaType.Black, FindBrush("BlackPreviewBorderBrush") },
            { ManaType.Red, FindBrush("RedPreviewBorderBrush") },
            { ManaType.Green, FindBrush("GreenPreviewBorderBrush") },
        };

        /// <summary>
        /// Pre-cached dictionary of frame brushes matching their respective mana types.
        /// </summary>
        protected static readonly Dictionary<ManaType, Brush> PreviewFrameBrushes = new Dictionary<ManaType, Brush>()
        {
            { ManaType.Generic, FindBrush("GenericPreviewFrameBrush") },
            { ManaType.White, FindBrush("WhitePreviewFrameBrush") },
            { ManaType.Blue, FindBrush("BluePreviewFrameBrush") },
            { ManaType.Black, FindBrush("BlackPreviewFrameBrush") },
            { ManaType.Red, FindBrush("RedPreviewFrameBrush") },
            { ManaType.Green, FindBrush("GreenPreviewFrameBrush") },
        };

        /// <summary>
        /// Pre-cached dictionary of information frame brushes matching their respective mana types.
        /// </summary>
        protected static readonly Dictionary<ManaType, Brush> PreviewInformationFrameBrushes = new Dictionary<ManaType, Brush>()
        {
            { ManaType.Generic, FindBrush("GenericPreviewInformationFrameBrush") },
            { ManaType.White, FindBrush("WhitePreviewInformationFrameBrush") },
            { ManaType.Blue, FindBrush("BluePreviewInformationFrameBrush") },
            { ManaType.Black, FindBrush("BlackPreviewInformationFrameBrush") },
            { ManaType.Red, FindBrush("RedPreviewInformationFrameBrush") },
            { ManaType.Green, FindBrush("GreenPreviewInformationFrameBrush") },
        };

        /// <summary>
        /// The pre-cached background brush used for multi-colored cards.
        /// </summary>
        protected static readonly Brush MultiColorBackgroundBrush = FindBrush("MultiColorPreviewBackgroundBrush");

        /// <summary>
        /// The pre-cached border brush used for multi-colored cards.
        /// </summary>
        protected static readonly Brush MultiColorBorderBrush = FindBrush("MultiColorPreviewBorderBrush");

        /// <summary>
        /// The pre-cached frame brush used for multi-colored cards.
        /// </summary>
        protected static readonly Brush MultiColorFrameBrush = FindBrush("MultiColorPreviewFrameBrush");

        /// <summary>
        /// The pre-cached information frame brush used for multi-colored cards.
        /// </summary>
        protected static readonly Brush MultiColorInformationFrameBrush = FindBrush("MultiColorPreviewInformationFrameBrush");

        /// <summary>
        /// Calculates the "Color Identity" of the card based on resolved mana costs and updates all WPF brushes used by the preview UI.
        /// </summary>
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

            // Colorless or Generic-only cards use the gray generic frame.
            if (manaTypesInCardCount == 0 || (manaTypesInCardCount == 1 && manaTypesInCard.ContainsKey(ManaType.Generic)))
            {
                m_PreviewBackgroundBrush = PreviewBackgroundBrushes[ManaType.Generic];
                m_PreviewBorderBrush = PreviewBorderBrushes[ManaType.Generic];
                m_PreviewFrameBrush = PreviewFrameBrushes[ManaType.Generic];
                m_PreviewInformationFrameBrush = PreviewInformationFrameBrushes[ManaType.Generic];
            }
            // Mono-colored cards (even if they also cost generic mana) use their specific color frame.
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
                // Multi-color cards use gold backgrounds, but their borders vary based on the exact color count.
                m_PreviewBackgroundBrush = MultiColorBackgroundBrush;

                // Exactly 2 colors (ignoring generic) get a split-color dual gradient border.
                if (manaTypesInCardCount == 2 || (manaTypesInCardCount == 3 && manaTypesInCard.ContainsKey(ManaType.Generic)))
                {
                    List<Brush> borderBrushes = new List<Brush>();
                    List<Brush> informationFrameBrushes = new List<Brush>();

                    foreach (ManaType manaType in manaTypesInCard.Keys)
                    {
                        if (manaType != ManaType.Generic)
                        {
                            borderBrushes.Add(PreviewBorderBrushes[manaType]);
                            informationFrameBrushes.Add(PreviewInformationFrameBrushes[manaType]);
                        }
                    }

                    m_PreviewBorderBrush = CreateGradient(borderBrushes[0], borderBrushes[1]);
                    m_PreviewInformationFrameBrush = CreateGradient(informationFrameBrushes[0], informationFrameBrushes[1]);
                }
                // 3 or more colors get the standard solid gold border.
                else
                { 
                    m_PreviewBorderBrush = MultiColorBorderBrush;
                    m_PreviewInformationFrameBrush = MultiColorInformationFrameBrush;
                }

                m_PreviewFrameBrush = MultiColorFrameBrush;
            }

            OnPropertyChanged(nameof(PreviewBackgroundBrush));
            OnPropertyChanged(nameof(PreviewBorderBrush));
            OnPropertyChanged(nameof(PreviewFrameBrush));
            OnPropertyChanged(nameof(PreviewInformationFrameBrush));
        }

        /// <summary>
        /// Creates a split two-color gradient brush used for rendering dual-color card borders.
        /// </summary>
        /// <param name="startBrush">The primary color brush.</param>
        /// <param name="endBrush">The secondary color brush.</param>
        /// <returns>A correctly mapped <see cref="LinearGradientBrush"/>.</returns>
        protected LinearGradientBrush CreateGradient(Brush startBrush, Brush endBrush)
        {
            Color startColor = ((SolidColorBrush)startBrush).Color;
            Color endColor = ((SolidColorBrush)endBrush).Color;

            return new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 0),
                GradientStops =
                {
                    new GradientStop(startColor, 0.00),
                    new GradientStop(startColor, 0.45),
                    new GradientStop(endColor, 0.55),
                    new GradientStop(endColor, 1.00)
                }
            };
        }

        /// <summary>
        /// Safely retrieves a brush from the WPF Application resources without crashing if the resource is missing.
        /// </summary>
        /// <param name="resourceKey">The dictionary key of the required brush.</param>
        /// <returns>The resolved brush, or <see cref="Brushes.Pink"/> if missing.</returns>
        protected static Brush FindBrush(string resourceKey)
        {
            return App.Current.TryFindResource(resourceKey) as Brush ?? Brushes.Pink;
        }

        /// <summary>
        /// Invoked externally when the underlying assigned parent card's name is modified.
        /// Triggers a UI refresh for the resolved parent name display.
        /// </summary>
        public void OnParentNameChanged()
        {
            OnPropertyChanged(nameof(ResolvedParentCardName));
        }

        /// <summary>
        /// Handles changes to mana entry view models and applies them to the underlying model.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event data.</param>
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

        /// <summary>
        /// Handles changes to description or flavor text view models and applies them to the underlying model.
        /// </summary>
        /// <param name="sender">The object that raised the event.</param>
        /// <param name="e">Event data.</param>
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

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

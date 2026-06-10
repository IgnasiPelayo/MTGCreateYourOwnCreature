
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model.Cards;
using MTGCreateYourOwnCreature.Model.Parsing;
using MTGCreateYourOwnCreature.ViewModel.Cards;
using MTGCreateYourOwnCreature.ViewModel.Commands;

namespace MTGCreateYourOwnCreature.ViewModel
{
    /// <summary>
    /// The primary ViewModel for the application window. 
    /// Manages the collection of loaded cards, file importing, selection state, and the inheritance graph.
    /// </summary>
    internal class MTGCreatureEditorVM : INotifyPropertyChanged
    {
        /// <summary>
        /// The observable collection of all loaded creature cards for UI binding.
        /// </summary>
        public ObservableCollection<MTGCreatureCardVM> Cards { get; set; }

        /// <summary>
        /// A mapping from raw <see cref="MTGCreatureCard"/> models to their corresponding <see cref="MTGCreatureCardVM"/> wrappers.
        /// Used to quickly resolve view models when traversing the model's inheritance chain.
        /// </summary>
        protected Dictionary<MTGCreatureCard, MTGCreatureCardVM> m_CardsToVM = new Dictionary<MTGCreatureCard, MTGCreatureCardVM>();

        /// <summary>
        /// The backing field for the <see cref="CurrentCard"/> property.
        /// </summary>
        protected MTGCreatureCardVM? m_CurrentCard = null;

        /// <summary>
        /// The currently selected card in the editor.
        /// Changing this property updates UI bindings and command execution states.
        /// </summary>
        public MTGCreatureCardVM? CurrentCard
        {
            get => m_CurrentCard;
            set
            {
                m_CurrentCard = value;
                OnPropertyChanged(nameof(CurrentCard));

                if (CurrentCard != null)
                {
                    CurrentCard.UpdateCollectorNumber(Cards.IndexOf(CurrentCard), Cards.Count);
                }
            }
        }

        /// <summary>
        /// The backing field for the <see cref="ImportCommand"/>.
        /// </summary>
        protected readonly RelayCommand m_ImportCommand;

        /// <summary>
        /// The command that opens a file dialog and imports creature cards from a text file.
        /// </summary>
        public ICommand ImportCommand => m_ImportCommand;

        /// <summary>
        /// The backing field for the <see cref="AddCreatureCommand"/>.
        /// </summary>
        protected readonly RelayCommand m_AddCreatureCommand;

        /// <summary>
        /// The command executed when the user clicks the "Add New Creature" button in the UI.
        /// Instantiates a new, default creature card, adds it to the collection, and automatically selects it for editing.
        /// </summary>
        public ICommand AddCreatureCommand => m_AddCreatureCommand;

        /// <summary>
        /// A dictionary tracking the inverse inheritance graph (mapping a parent card to all of its direct children).
        /// Used to efficiently cascade property updates down the tree.
        /// </summary>
        protected Dictionary<MTGCreatureCardVM, List<MTGCreatureCardVM>> m_Ancestors = new Dictionary<MTGCreatureCardVM, List<MTGCreatureCardVM>>();

        /// <summary>
        /// The visibility state of the delete confirmation modal.
        /// </summary>
        public Visibility DeleteModalVisibility { get; set; }

        /// <summary>
        /// The dynamically generated warning text displayed in the delete confirmation modal,
        /// which lists all specific child cards that will be affected by the deletion.
        /// </summary>
        public string DeleteWarningText { get; set; }

        /// <summary>
        /// The backing field for the <see cref="RemoveCreatureCommand"/>.
        /// </summary>
        protected readonly RelayCommand m_RemoveCreatureCommand;

        /// <summary>
        /// The command that initiates the deletion sequence for the currently active card.
        /// Bypasses the modal if the card has no inherited dependencies; otherwise, prompts for confirmation.
        /// </summary>
        public ICommand RemoveCreatureCommand => m_RemoveCreatureCommand;

        /// <summary>
        /// The backing field for the <see cref="CloseDeleteModalCommand"/>.
        /// </summary>
        protected readonly RelayCommand m_CloseDeleteModalCommand;

        /// <summary>
        /// The command executed to cancel the deletion and hide the confirmation modal.
        /// </summary>
        public ICommand CloseDeleteModalCommand => m_CloseDeleteModalCommand;

        /// <summary>
        /// The backing field for the <see cref="ConfirmRemoveCreatureCommand"/>.
        /// </summary>
        protected readonly RelayCommand m_ConfirmRemoveCreatureCommand;

        /// <summary>
        /// The command executed when the user explicitly confirms the destructive deletion operation from the modal.
        /// </summary>
        public ICommand ConfirmRemoveCreatureCommand => m_ConfirmRemoveCreatureCommand;

        /// <summary>
        /// The visibility state of the parent selection UI overlay.
        /// </summary>
        public Visibility ParentPickerVisibility { get; set; }

        /// <summary>
        /// The observable collection of valid parent cards that the current card can inherit from.
        /// </summary>
        public ObservableCollection<MTGCreatureCardVM> AvailableParentCards { get; set; }

        /// <summary>
        /// The virtual root card used to clear a card's inheritance.
        /// </summary>
        protected MTGCreatureCardVM BaseCreatureCard { get; set; }

        /// <summary>
        /// The backing field for the <see cref="SelectedParentCard"/> property.
        /// </summary>
        protected MTGCreatureCardVM? m_SelectedParentCard;

        /// <summary>
        /// The card chosen in the parent picker UI. 
        /// Setting this property automatically re-parents the <see cref="CurrentCard"/> and updates the inheritance graph.
        /// </summary>
        public MTGCreatureCardVM? SelectedParentCard
        {
            get => m_SelectedParentCard;
            set
            {
                if (m_SelectedParentCard == null || CurrentCard == null || value == null)
                {
                    return;
                }

                MTGCreatureCardVM newSelectedCard = value;
                if (m_SelectedParentCard == value || CurrentCard.Card.ParentCreatureCard == newSelectedCard.Card)
                {
                    OnParentPickerClosed();
                    return;
                }

                // Remove the current card from its old parent's child list.
                if (CurrentCard.Card.ParentCreatureCard != null)
                {
                    MTGCreatureCardVM parentCard = m_CardsToVM[CurrentCard.Card.ParentCreatureCard];
                    m_Ancestors[parentCard].Remove(CurrentCard);
                }
                
                CurrentCard.ChangeParent(newSelectedCard == BaseCreatureCard ? null : newSelectedCard.Card);

                // Add the current card to its new parent's child list.
                if (newSelectedCard != BaseCreatureCard)
                {
                    m_Ancestors[newSelectedCard].Add(CurrentCard);
                }

                OnPropertyChanged(nameof(Cards));
                OnPropertyChanged(nameof(CurrentCard));

                OnParentPickerClosed();
            }
        }

        /// <summary>
        /// The backing field for the <see cref="OpenParentPickerCommand"/>.
        /// </summary>
        protected readonly RelayCommand m_OpenParentPickerCommand;

        /// <summary>
        /// The command that populates and displays the parent picker UI.
        /// </summary>
        public ICommand OpenParentPickerCommand => m_OpenParentPickerCommand;

        /// <summary>
        /// The backing field for the <see cref="CloseParentPickerCommand"/>.
        /// </summary>
        protected readonly RelayCommand m_CloseParentPickerCommand;

        /// <summary>
        /// The command that hides the parent picker UI without making any changes.
        /// </summary>
        public ICommand CloseParentPickerCommand => m_CloseParentPickerCommand;

        /// <summary>
        /// Whether any modal overlay is currently visible.
        /// </summary>
        public bool IsModalOpen => DeleteModalVisibility == Visibility.Visible || ParentPickerVisibility == Visibility.Visible;

        /// <summary>
        /// Initializes a new instance of the <see cref="MTGCreatureEditorVM"/> class.
        /// Configures the base UI state, collections, and commands.
        /// </summary>
        public MTGCreatureEditorVM()
        {
            Cards = new ObservableCollection<MTGCreatureCardVM>();

            m_ImportCommand = new RelayCommand(_ =>
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Multiselect = false,
                    DefaultExt = ".txt",
                    Filter = "Text documents (.txt)|*.txt"
                };

                if (dialog.ShowDialog() == true)
                {
                    ImportFile(dialog.FileName);
                }
            });

            m_AddCreatureCommand = new RelayCommand(_ =>
            {
                MTGCreatureCard card = new MTGCreatureCard
                {
                    Name = "New Creature",
                    Category = Model.Category.CategoryType.Creature
                };

                MTGCreatureCardVM creatureCard = new MTGCreatureCardVM(card);
                creatureCard.PropertyChanged += OnCardChanged;

                Cards.Add(creatureCard);

                m_Ancestors[creatureCard] = new List<MTGCreatureCardVM>();

                CurrentCard = creatureCard;

                OnPropertyChanged(nameof(Cards));
                OnPropertyChanged(nameof(CurrentCard));
            });

            DeleteModalVisibility = Visibility.Collapsed;

            m_RemoveCreatureCommand = new RelayCommand(_ =>
            {
                if (CurrentCard == null)
                {
                    return;
                }

                // Look up all cards that inherit directly from the card we are trying to delete.
                List<MTGCreatureCardVM> ancestors = m_Ancestors[CurrentCard];
                int numberOfAncestors = ancestors.Count;

                if (numberOfAncestors == 0)
                {
                    // If no other cards inherit from this one, delete it immediately.
                    PerformRemove();
                }
                else
                {
                    // Show the modal to warn the user about breaking inheritance.
                    DeleteModalVisibility = Visibility.Visible;

                    // Dynamically build a readable list of all affected child cards.
                    DeleteWarningText = $"'{CurrentCard.Name}' is the parent of {numberOfAncestors} other creature(s).";
                    foreach (MTGCreatureCardVM ancestor in ancestors)
                    {
                        DeleteWarningText += $"\n  — '{ancestor.Name}'";
                    }
                    DeleteWarningText += "\n\nDeleting it will break their inheritance. Are you sure you want to proceed?";

                    OnPropertyChanged(nameof(DeleteModalVisibility));
                    OnPropertyChanged(nameof(DeleteWarningText));
                    OnPropertyChanged(nameof(IsModalOpen));
                }
            });

            m_CloseDeleteModalCommand = new RelayCommand(_ =>
            {
                DeleteModalVisibility = Visibility.Collapsed;

                OnPropertyChanged(nameof(DeleteModalVisibility));
                OnPropertyChanged(nameof(IsModalOpen));
            });

            m_ConfirmRemoveCreatureCommand = new RelayCommand(_ =>
            {
                PerformRemove();

                DeleteModalVisibility = Visibility.Collapsed;

                OnPropertyChanged(nameof(DeleteModalVisibility));
                OnPropertyChanged(nameof(IsModalOpen));
            });

            ParentPickerVisibility = Visibility.Collapsed;

            AvailableParentCards = new ObservableCollection<MTGCreatureCardVM>();
            BaseCreatureCard = new MTGCreatureCardVM(CreateBaseCreatureCard());

            m_SelectedParentCard = null;

            m_OpenParentPickerCommand = new RelayCommand(_ =>
            {
                OnParentPickerOpened();
            });

            m_CloseParentPickerCommand = new RelayCommand(_ =>
            {
                OnParentPickerClosed();
            });
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Parses a creature text file from the specified path, generates ViewModels, and builds the initial inheritance graph.
        /// </summary>
        /// <param name="filePath">The absolute path to the text file.</param>
        protected void ImportFile(string filePath)
        {
            Cards.Clear();
            m_CardsToVM.Clear();
            m_Ancestors.Clear();
            CurrentCard = null;

            List<MTGCreatureCard> cards = MTGCreaturesParser.Parse(filePath);
            foreach (MTGCreatureCard card in cards)
            {
                MTGCreatureCardVM creatureCard = new MTGCreatureCardVM(card);
                creatureCard.PropertyChanged += OnCardChanged;

                m_CardsToVM[card] = creatureCard;

                m_Ancestors[creatureCard] = new List<MTGCreatureCardVM>();

                Cards.Add(creatureCard);
            }

            if (cards.Count > 0)
            {
                CurrentCard = Cards[0];
            }

            foreach (MTGCreatureCardVM card in Cards)
            {
                if (card.Card.ParentCreatureCard != null)
                {
                    MTGCreatureCardVM parentCardVM = m_CardsToVM[card.Card.ParentCreatureCard];
                    m_Ancestors[parentCardVM].Add(card);
                }
            }
        }

        /// <summary>
        /// Traverses the inheritance chain to determine if a specific card is an ancestor of another.
        /// Used strictly to prevent circular inheritance loops in the UI.
        /// </summary>
        /// <param name="potentialParent">The card that might be higher up in the chain.</param>
        /// <param name="child">The starting card to trace upwards from.</param>
        /// <returns>True if the <paramref name="potentialParent"/> is found in the <paramref name="child"/>'s inheritance chain; otherwise, false.</returns>
        protected bool IsAncestorCard(MTGCreatureCard potentialParent, MTGCreatureCardVM child)
        {
            MTGCreatureCard? current = child.Card.ParentCreatureCard;

            while (current != null)
            {
                if (current == potentialParent)
                {
                    return true;
                }

                current = current.ParentCreatureCard;
            }

            return false;
        }

        /// <summary>
        /// Creates the virtual root parent used when a card does not inherit from an existing card in the database.
        /// </summary>
        /// <returns>A new base <see cref="MTGCreatureCard"/>.</returns>
        public static MTGCreatureCard CreateBaseCreatureCard()
        {
            MTGCreatureCard baseCreatureCard = new MTGCreatureCard();
            baseCreatureCard.Name = "Base Creature";

            return baseCreatureCard;
        }

        /// <summary>
        /// Executes the internal removal of the active card, safely handles UI selection fallback, 
        /// and repairs the inheritance tree by bridging orphaned children to their grandparent card.
        /// </summary>
        protected void PerformRemove()
        {
            MTGCreatureCardVM cardToRemove = CurrentCard;

            // Intelligently select an adjacent card in the list so the Inspector doesn't just go blank.
            int currentCardIndex = Cards.IndexOf(CurrentCard);
            if (currentCardIndex > 0)
            {
                // Select the previous card.
                CurrentCard = Cards[currentCardIndex - 1];
            }
            else
            {
                // Select the next card, or null if list is now empty.
                CurrentCard = Cards.Count > 1 ? Cards[1] : null;
            }
            
            Cards.Remove(cardToRemove);

            // Extract the "grandparent" card (the parent of the card we are deleting).
            MTGCreatureCard? newParent = cardToRemove.Card.ParentCreatureCard;

            // Re-parent all orphaned children to the grandparent, bridging the gap left by the deleted card.
            List<MTGCreatureCardVM> ancestors = m_Ancestors[cardToRemove];
            foreach (MTGCreatureCardVM ancestor in ancestors)
            {
                ancestor.ChangeParent(newParent);

                // Re-register these children into the dictionary under their new parent's tracking list.
                if (newParent != null)
                {
                    m_Ancestors[m_CardsToVM[newParent]].Add(ancestor);
                }
            }

            m_Ancestors.Remove(cardToRemove);

            if (cardToRemove.HasParentCard)
            {
                // Unregister the deleted card from its own parent's tracking list.
                m_Ancestors[m_CardsToVM[cardToRemove.Card.ParentCreatureCard]].Remove(cardToRemove);
            }

            OnPropertyChanged(nameof(Cards));
            OnPropertyChanged(nameof(CurrentCard));
        }

        /// <summary>
        /// Event handler that cascades updates to child cards when an inherited property changes on a parent card.
        /// </summary>
        /// <param name="sender">The source <see cref="MTGCreatureCardVM"/>.</param>
        /// <param name="e">Event data detailing the property change.</param>
        protected void OnCardChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not MTGCreatureCardVM creatureCard)
            {
                return;
            }

            if (e.PropertyName == nameof(MTGCreatureCardVM.Name))
            {
                foreach (MTGCreatureCardVM ancestorCard in m_Ancestors[creatureCard])
                {
                    ancestorCard.OnParentNameChanged();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.ResolvedTotalMana))
            {
                foreach (MTGCreatureCardVM ancestorCard in m_Ancestors[creatureCard])
                {
                    ancestorCard.RecalculateMana();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.Tags))
            {
                foreach (MTGCreatureCardVM ancestorCard in m_Ancestors[creatureCard])
                {
                    ancestorCard.UpdateTags();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.Keywords))
            {
                foreach (MTGCreatureCardVM ancestorCard in m_Ancestors[creatureCard])
                {
                    ancestorCard.UpdateKeywords();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.Description))
            {
                foreach (MTGCreatureCardVM ancestorCard in m_Ancestors[creatureCard])
                {
                    ancestorCard.UpdateDescription();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.FlavorText))
            {
                foreach (MTGCreatureCardVM ancestorCard in m_Ancestors[creatureCard])
                {
                    ancestorCard.UpdateFlavorText();
                }
            }
        }

        /// <summary>
        /// Prepares the parent picker UI by populating <see cref="AvailableParentCards"/> 
        /// while filtering out invalid options to prevent circular inheritance dependencies.
        /// </summary>
        protected void OnParentPickerOpened()
        {
            if (CurrentCard == null)
            {
                return;
            }

            ParentPickerVisibility = Visibility.Visible;

            AvailableParentCards.Clear();
            AvailableParentCards.Add(BaseCreatureCard);

            foreach (MTGCreatureCardVM card in Cards)
            {
                // Prevent circular inheritance: A card cannot inherit from itself, 
                // nor can it inherit from a card that is already one of its descendants.
                if (card != CurrentCard && !IsAncestorCard(CurrentCard.Card, card))
                {
                    AvailableParentCards.Add(card);
                }
            }

            m_SelectedParentCard = BaseCreatureCard;

            if (CurrentCard != null && CurrentCard.HasParentCard)
            {
                foreach (MTGCreatureCardVM card in Cards)
                {
                    if (card.Card == CurrentCard.Card.ParentCreatureCard)
                    {
                        m_SelectedParentCard = card;
                        break;
                    }
                }
            }

            OnPropertyChanged(nameof(SelectedParentCard));
            OnPropertyChanged(nameof(AvailableParentCards));
            OnPropertyChanged(nameof(ParentPickerVisibility));
            OnPropertyChanged(nameof(IsModalOpen));
        }

        /// <summary>
        /// Hides the parent picker UI and clears its temporary selection state to free up memory.
        /// </summary>
        protected void OnParentPickerClosed()
        {
            ParentPickerVisibility = Visibility.Collapsed;

            AvailableParentCards.Clear();

            m_SelectedParentCard = null;

            OnPropertyChanged(nameof(ParentPickerVisibility));
            OnPropertyChanged(nameof(IsModalOpen));
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="name">The name of the property that changed.</param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

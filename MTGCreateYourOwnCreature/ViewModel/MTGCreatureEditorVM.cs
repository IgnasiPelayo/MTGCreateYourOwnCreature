
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model.Cards;
using MTGCreateYourOwnCreature.ViewModel.Cards;
using MTGCreateYourOwnCreature.ViewModel.Parsing;
using MTGCreateYourOwnCreature.ViewModel.Commands;
using System.Diagnostics;

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
        /// Whether the active collection currently contains any loaded creature cards.
        /// </summary>
        public bool HasCards { get; set; }

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
        /// The command executed when the user requests to start a completely new creature set.
        /// </summary>
        public ICommand NewCollectionCommand { get; }

        /// <summary>
        /// The command executed to add a single new default creature card to the current collection.
        /// </summary>
        public ICommand NewCreatureCardCommand { get; }

        /// <summary>
        /// The command executed to open a file dialog and import a saved collection of creature cards.
        /// </summary>
        public ICommand OpenCollectionCommand { get; }

        /// <summary>
        /// The absolute file path of the currently loaded or most recently saved collection.
        /// </summary>
        protected string m_CollectionFilePath = string.Empty;

        /// <summary>
        /// The command executed to save the current collection. 
        /// </summary>
        public ICommand SaveCollectionCommand { get; }

        /// <summary>
        /// The dynamic display text bound to the "Save" menu item.
        /// </summary>
        public string SaveMenuText { get; set; }

        /// <summary>
        /// The command executed to explicitly prompt the user for a new file location to save the collection.
        /// </summary>
        public ICommand SaveCollectionAsCommand { get; }

        /// <summary>
        /// The dynamic display text bound to the "Save As..." menu item.
        /// </summary>
        public string SaveAsMenuText { get; set; }

        /// <summary>
        /// The command executed to shut down and exit the application.
        /// </summary>
        public ICommand ExitCommand { get; }

        /// <summary>
        /// The command executed to create an identical copy of the currently selected creature card.
        /// </summary>
        public ICommand DuplicateCreatureCardCommand { get; }

        /// <summary>
        /// The command executed to initiate the deletion process for the currently selected card.
        /// </summary>
        public ICommand DeleteCreatureCardCommand { get; }

        /// <summary>
        /// The command executed to open external help documentation.
        /// </summary>
        public ICommand HelpViewerCommand { get; }

        /// <summary>
        /// The command executed to open the developer's LinkedIn profile in the default web browser.
        /// </summary>
        public ICommand LinkedInCommand { get; }

        /// <summary>
        /// A dictionary tracking the inverse inheritance graph (mapping a parent card to all of its direct children).
        /// Used to efficiently cascade property updates down the tree.
        /// </summary>
        protected Dictionary<MTGCreatureCardVM, List<MTGCreatureCardVM>> m_Descendants = new Dictionary<MTGCreatureCardVM, List<MTGCreatureCardVM>>();

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
        /// The command executed to cancel the deletion and hide the confirmation modal.
        /// </summary>
        public ICommand CloseDeleteModalCommand { get; }

        /// <summary>
        /// The command executed to confirm and finalize the deletion of a card after passing the modal warning.
        /// </summary>
        public ICommand ConfirmDeleteCreatureCommand { get; }

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
                    m_Descendants[parentCard].Remove(CurrentCard);
                }
                
                CurrentCard.ChangeParent(newSelectedCard == BaseCreatureCard ? null : newSelectedCard.Card);

                // Add the current card to its new parent's child list.
                if (newSelectedCard != BaseCreatureCard)
                {
                    m_Descendants[newSelectedCard].Add(CurrentCard);
                }

                OnPropertyChanged(nameof(CurrentCard));

                OnParentPickerClosed();
            }
        }

        /// <summary>
        /// The command that populates and displays the parent picker UI.
        /// </summary>
        public ICommand OpenParentPickerCommand { get; }

        /// <summary>
        /// The command that hides the parent picker UI without making any changes.
        /// </summary>
        public ICommand CloseParentPickerCommand { get; }

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

            HasCards = false;

            NewCollectionCommand = new RelayCommand(_ =>
            {
                Cards.Clear();
                AddNewCreature();
            });

            NewCreatureCardCommand = new RelayCommand(_ =>
            {
                AddNewCreature();
            });

            OpenCollectionCommand = new RelayCommand(_ =>
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

                    m_CollectionFilePath = dialog.FileName;

                    SaveMenuText = $"Save {dialog.SafeFileName}";
                    SaveAsMenuText = SaveMenuText + " as...";

                    OnPropertyChanged(nameof(SaveMenuText));
                    OnPropertyChanged(nameof(SaveAsMenuText));
                }
            });

            SaveCollectionCommand = new RelayCommand(async _ =>
            {
                if (!HasCards)
                {
                    return;
                }

                if (string.IsNullOrEmpty(m_CollectionFilePath))
                {
                    OpenSaveFileDialog();
                }

                if (!string.IsNullOrEmpty(m_CollectionFilePath))
                {
                    await MTGCreaturesParser.Export(GetExportOrder(), m_CollectionFilePath);
                }
            });

            SaveMenuText = "Save collection";

            SaveCollectionAsCommand = new RelayCommand(async _ =>
            {
                if (!HasCards)
                {
                    return;
                }

                if (OpenSaveFileDialog())
                {
                    await MTGCreaturesParser.Export(GetExportOrder(), m_CollectionFilePath);
                }
            });

            SaveAsMenuText = SaveMenuText + " as...";

            ExitCommand = new RelayCommand(_ =>
            {
                Application.Current.Shutdown();
            });

            DuplicateCreatureCardCommand = new RelayCommand(_ =>
            {
                DuplicateCreature(CurrentCard);
            });

            DeleteCreatureCardCommand = new RelayCommand(_ =>
            {
                DeleteCreature(CurrentCard);
            });

            HelpViewerCommand = new RelayCommand(_ =>
            {
                Console.WriteLine("Open Help Viewer");
            });

            LinkedInCommand = new RelayCommand(_ =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "https://www.linkedin.com/in/ignasipelayo/",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Could not open LinkedIn page.{Environment.NewLine}{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            });

            DeleteModalVisibility = Visibility.Collapsed;
            DeleteWarningText = string.Empty;

            CloseDeleteModalCommand = new RelayCommand(_ =>
            {
                DeleteModalVisibility = Visibility.Collapsed;

                OnPropertyChanged(nameof(DeleteModalVisibility));
                OnPropertyChanged(nameof(IsModalOpen));
            });

            ConfirmDeleteCreatureCommand = new RelayCommand(_ =>
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

            OpenParentPickerCommand = new RelayCommand(_ =>
            {
                OnParentPickerOpened();
            });

            CloseParentPickerCommand = new RelayCommand(_ =>
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
            m_Descendants.Clear();
            CurrentCard = null;

            List<MTGCreatureCard> cards = MTGCreaturesParser.Parse(filePath);
            foreach (MTGCreatureCard card in cards)
            {
                MTGCreatureCardVM creatureCard = new MTGCreatureCardVM(card);
                creatureCard.PropertyChanged += OnCardChanged;

                m_CardsToVM[card] = creatureCard;

                m_Descendants[creatureCard] = new List<MTGCreatureCardVM>();

                Cards.Add(creatureCard);
            }

            if (cards.Count > 0)
            {
                CurrentCard = Cards[0];
                HasCards = true;
            }
            else
            {
                HasCards = false;
            }

            foreach (MTGCreatureCardVM card in Cards)
            {
                if (card.Card.ParentCreatureCard != null)
                {
                    MTGCreatureCardVM parentCardVM = m_CardsToVM[card.Card.ParentCreatureCard];
                    m_Descendants[parentCardVM].Add(card);
                }
            }

            OnPropertyChanged(nameof(HasCards));
            OnPropertyChanged(nameof(CurrentCard));
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
        /// Instantiates a new, default creature card, registers its event listeners, 
        /// adds it to the main collection, and automatically selects it for editing.
        /// </summary>
        protected void AddNewCreature()
        {
            MTGCreatureCard card = new MTGCreatureCard
            {
                Name = "New Creature",
                Category = Model.Category.CategoryType.Creature
            };

            MTGCreatureCardVM creatureCard = new MTGCreatureCardVM(card);
            creatureCard.PropertyChanged += OnCardChanged;

            Cards.Add(creatureCard);
            HasCards = true;
            m_CardsToVM[card] = creatureCard;

            m_Descendants[creatureCard] = new List<MTGCreatureCardVM>();

            CurrentCard = creatureCard;

            OnPropertyChanged(nameof(HasCards));
            OnPropertyChanged(nameof(CurrentCard));
        }

        /// <summary>
        /// Duplicates the specified card, registers its event listeners, adds it to the main,
        /// collection, and automatically selects it for editing.
        /// </summary>
        /// <param name="sourceVM">The view model wrapper of the card to duplicate.</param>
        protected void DuplicateCreature(MTGCreatureCardVM? sourceVM)
        {
            if (sourceVM == null)
            {
                return;
            }

            MTGCreatureCard duplicatedCard = new MTGCreatureCard(sourceVM.Card);
            
            MTGCreatureCardVM newCardVM = new MTGCreatureCardVM(duplicatedCard);
            newCardVM.PropertyChanged += OnCardChanged;

            int insertIndex = Cards.IndexOf(sourceVM) + 1;
            Cards.Insert(insertIndex, newCardVM);
            m_CardsToVM[duplicatedCard] = newCardVM;

            m_Descendants[newCardVM] = new List<MTGCreatureCardVM>();

            if (duplicatedCard.ParentCreatureCard != null)
            {
                MTGCreatureCardVM parentVM = m_CardsToVM[duplicatedCard.ParentCreatureCard];
                m_Descendants[parentVM].Add(newCardVM);
            }

            CurrentCard = newCardVM;

            OnPropertyChanged(nameof(CurrentCard));
        }

        /// <summary>
        /// Evaluates a card prior to deletion. If the card acts as a parent to others, 
        /// the user is prompted with a confirmation modal detailing the inheritance disruption.
        /// </summary>
        /// <param name="card">The specific <see cref="MTGCreatureCardVM"/> requested for deletion.</param>
        protected void DeleteCreature(MTGCreatureCardVM? card)
        {
            if (CurrentCard == null)
            {
                return;
            }

            // Look up all cards that inherit directly from the card we are trying to delete.
            List<MTGCreatureCardVM> descendants = m_Descendants[CurrentCard];
            int numberOfDescendants = descendants.Count;

            if (numberOfDescendants == 0)
            {
                // If no other cards inherit from this one, delete it immediately.
                PerformRemove();
            }
            else
            {
                // Show the modal to warn the user about breaking inheritance.
                DeleteModalVisibility = Visibility.Visible;

                // Dynamically build a readable list of all affected child cards.
                DeleteWarningText = $"'{CurrentCard.Name}' is the parent of {numberOfDescendants} other creature(s).";
                foreach (MTGCreatureCardVM descentant in descendants)
                {
                    DeleteWarningText += $"\n  — '{descentant.Name}'";
                }
                DeleteWarningText += "\n\nDeleting it will break their inheritance. Are you sure you want to proceed?";

                OnPropertyChanged(nameof(DeleteModalVisibility));
                OnPropertyChanged(nameof(DeleteWarningText));
                OnPropertyChanged(nameof(IsModalOpen));
            }
        }

        /// <summary>
        /// Executes the internal removal of the active card, safely handles UI selection fallback, 
        /// and repairs the inheritance tree by bridging orphaned children to their grandparent card.
        /// </summary>
        protected void PerformRemove()
        {
            if (CurrentCard == null)
            {
                return;
            }

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
            m_CardsToVM.Remove(cardToRemove.Card);

            // Extract the "grandparent" card (the parent of the card we are deleting).
            MTGCreatureCard? newParent = cardToRemove.Card.ParentCreatureCard;

            // Re-parent all orphaned children to the grandparent, bridging the gap left by the deleted card.
            List<MTGCreatureCardVM> descendants = m_Descendants[cardToRemove];
            foreach (MTGCreatureCardVM descendant in descendants)
            {
                descendant.ChangeParent(newParent);

                // Re-register these children into the dictionary under their new parent's tracking list.
                if (newParent != null)
                {
                    m_Descendants[m_CardsToVM[newParent]].Add(descendant);
                }
            }

            m_Descendants.Remove(cardToRemove);

            if (cardToRemove.HasParentCard)
            {
                // Unregister the deleted card from its own parent's tracking list.
                m_Descendants[m_CardsToVM[cardToRemove.Card.ParentCreatureCard]].Remove(cardToRemove);
            }

            OnPropertyChanged(nameof(CurrentCard));

            if (Cards.Count == 0)
            {
                HasCards = false;
                OnPropertyChanged(nameof(HasCards));

                m_CollectionFilePath = string.Empty;

                SaveMenuText = "Save collection";
                SaveAsMenuText = SaveMenuText + " as...";

                OnPropertyChanged(nameof(SaveMenuText));
                OnPropertyChanged(nameof(SaveAsMenuText));
            }
        }

        /// <summary>
        /// Performs a topological sort of the active collection based on the inheritance graph.
        /// Ensures that parent cards are ordered before their children to satisfy parsing constraints during export.
        /// </summary>
        /// <returns>A flat list of <see cref="MTGCreatureCard"/> models sorted chronologically by inheritance dependency.</returns>
        protected List<MTGCreatureCard> GetExportOrder()
        {
            // Topological Sort (Kahn's Algorithm)
            // Calculate the in-degree (number of dependencies) for each card.
            Dictionary<MTGCreatureCardVM, int> inDegree = new Dictionary<MTGCreatureCardVM, int>();

            foreach (MTGCreatureCardVM card in Cards)
            {
                inDegree[card] = 0;
            }

            foreach (KeyValuePair<MTGCreatureCardVM, List<MTGCreatureCardVM>> descendantInformation in m_Descendants)
            {
                foreach (MTGCreatureCardVM descendant in descendantInformation.Value)
                {
                    // Each child gets +1 degree because it depends on its parent.
                    ++inDegree[descendant];
                }
            }

            Queue<MTGCreatureCardVM> queue = new Queue<MTGCreatureCardVM>();

            foreach (MTGCreatureCardVM card in Cards)
            {
                // Cards with an in-degree of 0 are root cards (they have no dependencies). Queue them first.
                if (inDegree[card] == 0)
                {
                    queue.Enqueue(card);
                }
            }

            List<MTGCreatureCard> orderedCards = new List<MTGCreatureCard>();

            while (queue.Count > 0)
            {
                // Extract a card whose dependencies have all been met and add it to the final sorted output list.
                MTGCreatureCardVM card = queue.Dequeue();

                orderedCards.Add(card.Card);

                // Now that this parent is processed, "release" its children by reducing their in-degree by 1.
                foreach (MTGCreatureCardVM descendant in m_Descendants[card])
                {
                    --inDegree[descendant];

                    // If the child's in-degree hits 0, all its dependencies are resolved, and it is ready to be queued.
                    if (inDegree[descendant] == 0)
                    {
                        queue.Enqueue(descendant);
                    }
                }
            }

            return orderedCards;
        }

        /// <summary>
        /// Prompts the user to select a destination file path using the standard Windows Save dialog.
        /// </summary>
        /// <returns>True if the user successfully selected a file path; otherwise, false.</returns>
        protected bool OpenSaveFileDialog()
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Filter = "Creature Collection (*.txt)|*.txt";
            dialog.DefaultExt = ".txt";

            if (dialog.ShowDialog() == true)
            {
                m_CollectionFilePath = dialog.FileName;

                SaveMenuText = $"Save {dialog.SafeFileName}";
                SaveAsMenuText = SaveMenuText + " as...";

                OnPropertyChanged(nameof(SaveMenuText));
                OnPropertyChanged(nameof(SaveAsMenuText));

                return true;
            }

            return false;
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
                foreach (MTGCreatureCardVM descendantCard in m_Descendants[creatureCard])
                {
                    descendantCard.OnParentNameChanged();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.ResolvedTotalMana))
            {
                foreach (MTGCreatureCardVM descendantCard in m_Descendants[creatureCard])
                {
                    descendantCard.RecalculateMana();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.Tags))
            {
                foreach (MTGCreatureCardVM descendantCard in m_Descendants[creatureCard])
                {
                    descendantCard.UpdateTags();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.Keywords))
            {
                foreach (MTGCreatureCardVM descendantCard in m_Descendants[creatureCard])
                {
                    descendantCard.UpdateKeywords();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.Description))
            {
                foreach (MTGCreatureCardVM descendantCard in m_Descendants[creatureCard])
                {
                    descendantCard.UpdateDescription();
                }
            }
            else if (e.PropertyName == nameof(MTGCreatureCardVM.FlavorText))
            {
                foreach (MTGCreatureCardVM descendantCard in m_Descendants[creatureCard])
                {
                    descendantCard.UpdateFlavorText();
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

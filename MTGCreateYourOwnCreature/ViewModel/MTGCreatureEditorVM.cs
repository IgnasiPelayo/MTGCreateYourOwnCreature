using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.ViewModel.Cards;
using MTGCreateYourOwnCreature.ViewModel.Helpers;
using MTGCreateYourOwnCreature.ViewModel.Commands;

namespace MTGCreateYourOwnCreature.ViewModel
{
    internal class MTGCreatureEditorVM : INotifyPropertyChanged
    {
        public ObservableCollection<MTGCreatureCardVM> Cards { get; set; }

        protected Dictionary<MTGCreatureCard, MTGCreatureCardVM> m_CardsToVM = new Dictionary<MTGCreatureCard, MTGCreatureCardVM>();


        protected MTGCreatureCardVM? m_CurrentCard = null;
        public MTGCreatureCardVM? CurrentCard
        {
            get => m_CurrentCard;
            set
            {
                m_CurrentCard = value;
                OnPropertyChanged(nameof(CurrentCard));
            }
        }

        protected readonly ICommand m_ImportCommand;
        public ICommand ImportCommand => m_ImportCommand;


        protected Dictionary<MTGCreatureCardVM, List<MTGCreatureCardVM>> m_Ancestors = new Dictionary<MTGCreatureCardVM, List<MTGCreatureCardVM>>();


        public Visibility ParentPickerVisibility { get; set; }

        public ObservableCollection<MTGCreatureCardVM> AvailableParentCards { get; set; }

        protected MTGCreatureCardVM BaseCreatureCard { get; set; }


        protected MTGCreatureCardVM? m_SelectedParentCard;
        public MTGCreatureCardVM? SelectedParentCard
        {
            get => m_SelectedParentCard;
            set
            {
                if (m_SelectedParentCard == null || CurrentCard == null || value == null)
                {
                    return;
                }

                MTGCreatureCardVM? newSelectedCard = value;
                if (m_SelectedParentCard == value || (newSelectedCard != null && CurrentCard.Card.ParentCreatureCard == newSelectedCard.Card))
                {
                    OnParentPickerClosed();
                    return;
                }

                if (CurrentCard.Card.ParentCreatureCard != null)
                {
                    MTGCreatureCardVM parentCard = m_CardsToVM[CurrentCard.Card.ParentCreatureCard];
                    m_Ancestors[parentCard].Remove(CurrentCard);
                }
                
                CurrentCard.ChangeParent(newSelectedCard == BaseCreatureCard ? null : newSelectedCard.Card);

                if (newSelectedCard != BaseCreatureCard)
                {
                    m_Ancestors[newSelectedCard].Add(CurrentCard);
                }

                OnPropertyChanged(nameof(Cards));
                OnPropertyChanged(nameof(CurrentCard));

                OnParentPickerClosed();
            }
        }



        protected readonly ICommand m_OpenParentPickerCommand;
        public ICommand OpenParentPickerCommand => m_OpenParentPickerCommand;

        protected readonly ICommand m_CloseParentPickerCommand;
        public ICommand CloseParentPickerCommand => m_CloseParentPickerCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        public MTGCreatureEditorVM()
        {
            Cards = new ObservableCollection<MTGCreatureCardVM>();

            m_ImportCommand = new RelayCommand(_ =>
            {
                OpenFileDialog dialog = new OpenFileDialog()
                {
                    Multiselect = true,
                    DefaultExt = ".txt",
                    Filter = "Text documents (.txt)|*.txt"
                };

                if (dialog.ShowDialog() == true)
                {
                    ImportFile(dialog.FileName);
                }
            });


            ParentPickerVisibility = Visibility.Collapsed;

            AvailableParentCards = new ObservableCollection<MTGCreatureCardVM>();
            BaseCreatureCard = new MTGCreatureCardVM(MTGCreatureCard.CreateBaseCreatureCard());

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


        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        protected void ImportFile(string filePath)
        {
            Cards.Clear();

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

        protected void OnCardChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is not MTGCreatureCardVM creatureCard)
            {
                return;
            }

            if (e.PropertyName == nameof(MTGCreatureCardVM.ResolvedTotalMana))
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

        protected void OnParentPickerOpened()
        {
            ParentPickerVisibility = Visibility.Visible;

            AvailableParentCards.Clear();
            AvailableParentCards.Add(BaseCreatureCard);

            foreach (MTGCreatureCardVM card in Cards)
            {
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
        }


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


        protected void OnParentPickerClosed()
        {
            ParentPickerVisibility = Visibility.Collapsed;

            AvailableParentCards.Clear();

            m_SelectedParentCard = null;

            OnPropertyChanged(nameof(ParentPickerVisibility));
        }
    }
}

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


        public ImportCommand ImportCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MTGCreatureEditorVM()
        {
            Cards = new ObservableCollection<MTGCreatureCardVM>();

            ImportCommand = new ImportCommand(this);
        }


        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        public void ImportFile(string filePath)
        {
            Cards.Clear();

            List<MTGCreatureCard> cards = MTGCreaturesParser.Parse(filePath);
            foreach (MTGCreatureCard card in cards)
            {
                Cards.Add(new MTGCreatureCardVM(card));
            }

            if (cards.Count > 0)
            {
                CurrentCard = Cards[0];
            }
        }
    }
}

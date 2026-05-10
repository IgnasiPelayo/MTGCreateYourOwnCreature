using System.ComponentModel;
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.ViewModel.Commands;
using MTGCreateYourOwnCreature.ViewModel.Helpers;

namespace MTGCreateYourOwnCreature.ViewModel
{
    internal class MTGCreatureEditorVM : INotifyPropertyChanged
    {
        public ObservableCollection<MTGCreatureCard> Cards { get; set; }


        protected MTGCreatureCard? m_CurrentCard;
        public MTGCreatureCard? CurrentCard
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
            Cards = new ObservableCollection<MTGCreatureCard>();

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
            for (int i = 0; i < cards.Count; ++i)
            {
                Cards.Add(cards[i]);
            }

            if (cards.Count > 0)
            {
                CurrentCard = Cards[0];
            }
        }
    }
}

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

        public MTGCreatureCard? CurrentCard { get; set; }


        public ImportCommand ImportCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MTGCreatureEditorVM()
        {
            Cards = new ObservableCollection<MTGCreatureCard>();

            ImportCommand = new ImportCommand(this);
        }

        public void ImportFile(string filePath)
        {
            Cards.Clear();

            List<MTGCreatureCard> cards = MTGCreaturesParser.Parse(filePath);
            for (int i = 0; i < cards.Count; ++i)
            {
                Cards.Add(cards[i]);
            }
        }
    }
}

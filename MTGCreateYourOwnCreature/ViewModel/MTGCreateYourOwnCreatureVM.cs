using MTGCreateYourOwnCreature.ViewModel.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MTGCreateYourOwnCreature.ViewModel
{
    internal class MTGCreateYourOwnCreatureVM : INotifyPropertyChanged
    {
        public ImportCommand ImportCommand { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MTGCreateYourOwnCreatureVM()
        {
            ImportCommand = new ImportCommand(this);
        }

        public void ImportFile(string filePath)
        {
            System.Diagnostics.Debug.WriteLine("Importing...");
        }
    }
}

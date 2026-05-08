using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MTGCreateYourOwnCreature.ViewModel.Commands
{
    internal class ImportCommand : ICommand
    {
        public MTGCreatureEditorVM VM { get; set; }

        public event EventHandler? CanExecuteChanged;

        public ImportCommand(MTGCreatureEditorVM vm)
        {
            VM = vm;
        }

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                Multiselect = true,
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };

            if (dialog.ShowDialog() == true)
            {
                VM.ImportFile(dialog.FileName);
            }
        }
    }
}

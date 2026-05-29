using System.ComponentModel;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGTraitEntryVM : INotifyPropertyChanged
    {
        public string Value { get; }

        public bool IsInherited { get; }


        public MTGTraitEntryVM(string value, bool isInherited)
        {
            Value = value;
            IsInherited = isInherited;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

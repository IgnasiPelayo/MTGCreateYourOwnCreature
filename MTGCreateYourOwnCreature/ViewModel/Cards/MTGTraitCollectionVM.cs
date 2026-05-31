using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.ViewModel.Commands;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGTraitCollectionVM : INotifyPropertyChanged
    {
        public ObservableCollection<MTGTraitEntryVM> Traits { get; set; }

        protected string m_NewTrait = string.Empty;

        public string NewTrait
        {
            get => m_NewTrait;
            set
            {
                if (m_NewTrait == value)
                {
                    return;
                }

                m_NewTrait = value;

                OnPropertyChanged(nameof(NewTrait));
            }
        }
        
        public ICommand AddCommand { get; }
        protected readonly Action<MTGTraitEntryVM> m_OnAdd;

        public ICommand RemoveCommand { get; }
        protected readonly Action<MTGTraitEntryVM> m_OnRemove;


        public MTGTraitCollectionVM(IReadOnlyCollection<MTGTraitEntryVM> traits, Func<string, bool> canAdd, Action<MTGTraitEntryVM> onAdd, Action<MTGTraitEntryVM> onRemove)
        {
            Traits = new ObservableCollection<MTGTraitEntryVM>(traits);

            AddCommand = new RelayCommand(_ => Add(canAdd, onAdd));
            m_OnAdd = onAdd;

            RemoveCommand = new RelayCommand(param =>
            {
                if (param is not MTGTraitEntryVM trait)
                {
                    return;
                }

                Remove(trait);
            });
            m_OnRemove = onRemove;
        }


        protected void Add(Func<string, bool> canAdd, Action<MTGTraitEntryVM> onAdd)
        {
            string trait = NewTrait?.Trim();

            if (string.IsNullOrWhiteSpace(trait) || !canAdd(trait))
            {
                return;
            }

            MTGTraitEntryVM traitEntry = new MTGTraitEntryVM(trait, false);

            Traits.Add(traitEntry);

            onAdd(traitEntry);

            NewTrait = string.Empty;
        }

        protected void Remove(MTGTraitEntryVM trait)
        {
            if (trait == null)
            {
                return;
            }

            Traits.Remove(trait);

            m_OnRemove(trait);
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

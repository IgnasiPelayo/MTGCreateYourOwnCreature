
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.ObjectModel;

using MTGCreateYourOwnCreature.ViewModel.Commands;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    /// <summary>
    /// ViewModel for managing a collection of string-based traits.
    /// Handles the UI logic for adding new traits via an input field and removing existing local traits.
    /// </summary>
    public class MTGTraitCollectionVM : INotifyPropertyChanged
    {
        /// <summary>
        /// The observable collection of trait entries used for UI binding.
        /// </summary>
        public ObservableCollection<MTGTraitEntryVM> Traits { get; set; }

        /// <summary>
        /// The backing field for the <see cref="NewTrait"/> property.
        /// </summary>
        protected string m_NewTrait = string.Empty;

        /// <summary>
        /// The text currently entered in the UI input field for a new trait.
        /// </summary>
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

        /// <summary>
        /// The command responsible for validating and adding a new trait to the collection.
        /// </summary>
        public ICommand AddCommand { get; }

        /// <summary>
        /// The callback action invoked when a new trait is successfully added to the underlying model.
        /// </summary>
        protected readonly Action<MTGTraitEntryVM> m_OnAdd;

        /// <summary>
        /// The command responsible for removing an existing local trait from the collection.
        /// </summary>
        public ICommand RemoveCommand { get; }

        /// <summary>
        /// The callback action invoked when a trait is successfully removed from the underlying model.
        /// </summary>
        protected readonly Action<MTGTraitEntryVM> m_OnRemove;

        /// <summary>
        /// Initializes a new instance of the <see cref="MTGTraitCollectionVM"/> class.
        /// </summary>
        /// <param name="traits">The initial collection of traits (both inherited and local) to display.</param>
        /// <param name="canAdd">A validation function determining if a new trait string is allowed to be added.</param>
        /// <param name="onAdd">The callback executed after a trait is successfully added.</param>
        /// <param name="onRemove">The callback executed after a local trait is successfully removed.</param>
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

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Trims and validates the text currently in the <see cref="NewTrait"/> field, 
        /// adding it to the collection if valid, invoking the add callback, and clearing the input.
        /// </summary>
        /// <param name="canAdd">The validation function to check the new trait string.</param>
        /// <param name="onAdd">The callback to invoke upon successful addition.</param>
        protected void Add(Func<string, bool> canAdd, Action<MTGTraitEntryVM> onAdd)
        {
            string trait = NewTrait.Trim();

            if (string.IsNullOrWhiteSpace(trait) || !canAdd(trait))
            {
                return;
            }

            MTGTraitEntryVM traitEntry = new MTGTraitEntryVM(trait, false);

            Traits.Add(traitEntry);

            onAdd(traitEntry);

            NewTrait = string.Empty;
        }

        /// <summary>
        /// Removes the specified trait entry from the observable collection and invokes the removal callback.
        /// </summary>
        /// <param name="trait">The specific trait entry view model to remove.</param>
        protected void Remove(MTGTraitEntryVM trait)
        {
            Traits.Remove(trait);

            m_OnRemove(trait);
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

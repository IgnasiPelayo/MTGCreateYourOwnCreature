
using System.ComponentModel;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    /// <summary>
    /// ViewModel for a text field that can use inherited text or its own override.
    /// Typically bound to UI elements representing the rules description or flavor text of a card.
    /// </summary>
    public class MTGInformationEntryVM : INotifyPropertyChanged
    {
        /// <summary>
        /// The explicit text value defined for this specific card.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The text value inherited from the parent card.
        /// </summary>
        public string InheritedValue { get; set; }

        /// <summary>
        /// The backing field for the <see cref="OverridesValue"/> property.
        /// </summary>
        protected bool m_OverridesValue = false;

        /// <summary>
        /// Whether this entry explicitly overrides the inherited text.
        /// When true, the UI will display and edit <see cref="Value"/> instead of <see cref="InheritedValue"/>.
        /// </summary>
        public bool OverridesValue
        {
            get => m_OverridesValue;
            set
            {
                if (m_OverridesValue == value)
                {
                    return;
                }

                m_OverridesValue = value;

                OnPropertyChanged(nameof(OverridesValue));
                OnPropertyChanged(nameof(IsReadOnly));
                OnPropertyChanged(nameof(ResolvedValue));
                OnPropertyChanged(nameof(HasValue));
            }
        }

        /// <summary>
        /// Whether the UI text field should be locked.
        /// Returns true if the entry is inheriting its value from a parent card.
        /// </summary>
        public bool IsReadOnly => !OverridesValue;

        /// <summary>
        /// The final text to display in the UI. 
        /// Returns the overridden <see cref="Value"/> if active; otherwise, returns the <see cref="InheritedValue"/>.
        /// </summary>
        public string ResolvedValue
        {
            get => OverridesValue ? Value : InheritedValue;
            set
            {
                if (OverridesValue && Value != value)
                {
                    Value = value;

                    OnPropertyChanged(nameof(Value));
                    OnPropertyChanged(nameof(ResolvedValue));
                    OnPropertyChanged(nameof(HasValue));
                }
            }
        }

        /// <summary>
        /// Whether there is any valid text to display (either explicitly set or inherited).
        /// Used by the UI to collapse empty text blocks or dividers.
        /// </summary>
        public bool HasValue => !string.IsNullOrWhiteSpace(ResolvedValue);

        /// <summary>
        /// Initializes a new instance of the <see cref="MTGInformationEntryVM"/> class.
        /// </summary>
        /// <param name="value">The explicit text value for this card.</param>
        /// <param name="inheritedValue">The fallback text value from the parent card.</param>
        /// <param name="overridesValue">True if this card should ignore the inherited value.</param>
        public MTGInformationEntryVM(string value, string inheritedValue, bool overridesValue)
        {
            m_OverridesValue = overridesValue;

            Value = value;
            InheritedValue = inheritedValue;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Refreshes the UI state when the parent card's inherited text changes.
        /// </summary>
        public void UpdateInheritance()
        {
            OnPropertyChanged(nameof(InheritedValue));
            OnPropertyChanged(nameof(ResolvedValue));
            OnPropertyChanged(nameof(HasValue));
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

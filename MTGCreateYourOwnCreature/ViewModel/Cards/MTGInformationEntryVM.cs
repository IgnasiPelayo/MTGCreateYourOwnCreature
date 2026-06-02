
using System.ComponentModel;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGInformationEntryVM : INotifyPropertyChanged
    {
        public string Value { get; set; }

        public string InheritedValue { get; set; }

        protected bool m_OverridesValue = false;

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

        public bool IsReadOnly => !OverridesValue;

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

        public bool HasValue => !string.IsNullOrWhiteSpace(ResolvedValue);
            
        public MTGInformationEntryVM(string value, string inheritedValue, bool overridesValue)
        {
            m_OverridesValue = overridesValue;

            Value = value;
            InheritedValue = inheritedValue;
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpdateInheritance()
        {
            OnPropertyChanged(nameof(InheritedValue));
        }
    }
}

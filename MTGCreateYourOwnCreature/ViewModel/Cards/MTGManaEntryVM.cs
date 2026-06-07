using System.ComponentModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Rendering;
using MTGCreateYourOwnCreature.Model.Mana;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    /// <summary>
    /// ViewModel for editing the required amount of one specific mana type in the card inspector.
    /// </summary>
    public class MTGManaEntryVM : INotifyPropertyChanged
    {
        /// <summary>
        /// The backing field for the <see cref="Type"/> property.
        /// </summary>
        protected readonly ManaType m_Type;

        /// <summary>
        /// The specific type of mana this entry represents (e.g., Green, Blue, Generic).
        /// </summary>
        public ManaType Type => m_Type;

        /// <summary>
        /// The backing field for the <see cref="Value"/> property.
        /// </summary>
        protected int m_Value;

        /// <summary>
        /// The explicit amount of this mana type required for the card.
        /// Automatically clamped to a minimum of 0.
        /// </summary>
        public int Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (value == m_Value)
                {
                    return;
                }

                m_Value = value;
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(ManaSymbols));
            }
        }

        /// <summary>
        /// The backing field for the <see cref="InheritedValue"/> property.
        /// </summary>
        protected int m_InheritedValue;

        /// <summary>
        /// The amount of this mana type inherited from a parent card.
        /// Automatically clamped to a minimum of 0.
        /// </summary>
        public int InheritedValue
        {
            get => m_InheritedValue;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                if (value == m_InheritedValue)
                {
                    return;
                }

                m_InheritedValue = value;
                OnPropertyChanged(nameof(InheritedManaSymbols));
            }
        }

        /// <summary>
        /// Static preview symbol for this mana type, typically used as a row label in the inspector UI.
        /// </summary>
        public MTGManaSymbolVM PreviewManaSymbol => ManaRenderService.CreatePreviewSymbol(m_Type);

        /// <summary>
        /// List of bindable visual mana symbols representing the explicitly set cost.
        /// Automatically formatted by the <see cref="ManaRenderService"/>.
        /// </summary>
        public IReadOnlyList<MTGManaSymbolVM> ManaSymbols => ManaRenderService.CreateSymbols(new Dictionary<ManaType, int> { { m_Type, m_Value } });

        /// <summary>
        /// List of bindable visual mana symbols representing the inherited cost.
        /// Automatically formatted as inherited symbols by the <see cref="ManaRenderService"/>.
        /// </summary>
        public IReadOnlyList<MTGManaSymbolVM> InheritedManaSymbols => ManaRenderService.CreateSymbols(new Dictionary<ManaType, int> { { m_Type, m_InheritedValue } }, true);

        /// <summary>
        /// Initializes a new instance of the <see cref="MTGManaEntryVM"/> class.
        /// </summary>
        /// <param name="type">The type of mana this entry will represent.</param>
        /// <param name="value">The explicit cost amount for this mana type.</param>
        /// <param name="inheritedValue">The inherited cost amount from the parent card.</param>
        public MTGManaEntryVM(ManaType type, int value, int inheritedValue)
        {
            m_Type = type;

            m_Value = value;
            m_InheritedValue = inheritedValue;
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;

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

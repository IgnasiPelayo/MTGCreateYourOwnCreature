using System.ComponentModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Rendering;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGManaEntryVM : INotifyPropertyChanged
    {
        protected readonly ManaType m_Type;
        public ManaType Type => m_Type;

        protected int m_Value;
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

        protected readonly int m_InheritedValue;

        public MTGManaEntryVM(ManaType type, int value, int inheritedValue)
        {
            m_Type = type;

            m_Value = value;
            m_InheritedValue = inheritedValue;
        }

        public MTGManaSymbol PreviewManaSymbol => ManaRenderService.CreatePreviewSymbol(m_Type);
        public IReadOnlyList<MTGManaSymbol> ManaSymbols => ManaRenderService.CreateSymbols(new Dictionary<ManaType, int> { { m_Type, m_Value } });
        public IReadOnlyList<MTGManaSymbol> InheritedManaSymbols => ManaRenderService.CreateSymbols(new Dictionary<ManaType, int> { { m_Type, m_InheritedValue } });


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

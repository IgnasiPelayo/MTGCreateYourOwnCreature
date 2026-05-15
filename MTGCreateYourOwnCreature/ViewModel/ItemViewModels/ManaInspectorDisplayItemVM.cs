using System.Windows.Media;
using System.ComponentModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;

namespace MTGCreateYourOwnCreature.ViewModel.ItemViewModels
{
    public class ManaInspectorDisplayItemVM : INotifyPropertyChanged
    {
        protected MTGCreatureCard m_Card;

        public ManaType ManaType { get; }

        public MTGManaSymbol DisplaySymbol { get; }

        public int Value 
        {
            get
            {
                return m_Card.Mana[ManaType];
            }

            set
            {
                m_Card.Mana[ManaType] = Math.Max(0, value);

                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(Symbols));
            }
        }


        public List<MTGManaSymbol> Symbols { get => CreateSymbols(Value); }


        public List<MTGManaSymbol> InheritedSymbols 
        {
            get
            {
                if (m_Card.ParentCreatureCard == null)
                {
                    return new List<MTGManaSymbol>();
                }

                return CreateSymbols(m_Card.ParentCreatureCard.Mana[ManaType]);
            }
        }


        public ManaInspectorDisplayItemVM(MTGCreatureCard card, ManaType manaType, Brush brush)
        {
            m_Card = card;

            ManaType = manaType;

            DisplaySymbol = new MTGManaSymbol("X", brush);
        }


        public event PropertyChangedEventHandler? PropertyChanged;


        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }


        protected List<MTGManaSymbol> CreateSymbols(int value)
        {
            if (ManaType == ManaType.Colorless)
            {
                return new List<MTGManaSymbol>() { new MTGManaSymbol(value.ToString(), DisplaySymbol.Brush) };
            }

            List<MTGManaSymbol> symbols = new List<MTGManaSymbol>();
            for (int i = 0; i < value; ++i)
            {
                symbols.Add(new MTGManaSymbol("", DisplaySymbol.Brush));
            }

            return symbols;
        }
    }
}

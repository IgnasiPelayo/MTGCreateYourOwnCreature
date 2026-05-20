using System.ComponentModel;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Rendering;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGManaEntryVM : INotifyPropertyChanged
    {
        protected readonly MTGCreatureCardVM m_Owner;
        protected readonly ManaType m_Type;


        public ManaType Type => m_Type;

        public MTGManaEntryVM(MTGCreatureCardVM owner, ManaType type)
        {
            m_Owner = owner;
            m_Type = type;
        }

        public int Value
        {
            get
            {
                return m_Owner.Card.Mana[m_Type];
            }
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                m_Owner.Card.Mana[m_Type] = value;
                OnPropertyChanged(nameof(Value));
                OnPropertyChanged(nameof(ManaSymbols));

                m_Owner.NotifyManaChanged(m_Type);
            }
        }

        public MTGManaSymbol PreviewManaSymbol => ManaRenderService.CreatePreviewSymbol(m_Type);
        public IReadOnlyList<MTGManaSymbol> ManaSymbols => ManaRenderService.CreateSymbols(new Dictionary<ManaType, int> { { m_Type, m_Owner.Card.Mana[m_Type] } });
        public IReadOnlyList<MTGManaSymbol> InheritedManaSymbols => ManaRenderService.CreateSymbols(new Dictionary<ManaType, int> { { m_Type, m_Owner.Card.ParentCreatureCard?.Mana[m_Type] ?? 0 } });


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

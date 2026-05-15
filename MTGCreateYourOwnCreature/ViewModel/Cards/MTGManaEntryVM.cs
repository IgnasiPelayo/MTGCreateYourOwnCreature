using MTGCreateYourOwnCreature.Model.Mana;

namespace MTGCreateYourOwnCreature.ViewModel.Cards
{
    public class MTGManaEntryVM
    {
        public ManaType ManaType { get; set; }

        public int Value { get; set; }


        public MTGManaEntryVM(ManaType type, int value)
        {
            ManaType = type;
            Value = value;
        }
    }
}

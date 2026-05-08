using System.Windows.Media;

namespace MTGCreateYourOwnCreature.Model
{
    public class MTGManaSymbol
    {
        public String Value { get; set; }

        public Brush Fill { get; set; }

        public MTGManaSymbol(String value, Brush fill) 
        {
            Value = value;
            Fill = fill;
        }
    }
}

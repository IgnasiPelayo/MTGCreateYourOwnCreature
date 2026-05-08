using System.Windows.Media;

namespace MTGCreateYourOwnCreature.Model
{
    public class MTGManaSymbol
    {
        public String Value { get; set; }

        public Brush? Brush { get; set; }

        public MTGManaSymbol(String value, Brush? brush) 
        {
            Value = value;
            Brush = brush;
        }
    }
}

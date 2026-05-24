using System.Windows.Media;

namespace MTGCreateYourOwnCreature.Model
{
    public class MTGManaSymbol
    {
        public String Value { get; set; }

        public Brush? Brush { get; set; }

        public bool IsInherited { get; set; }

        public MTGManaSymbol(String value, Brush? brush, bool isInherited = false)
        {
            Value = value;
            Brush = brush;
            IsInherited = isInherited;
        }
    }
}

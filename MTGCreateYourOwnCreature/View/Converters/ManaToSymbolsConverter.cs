using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

using MTGCreateYourOwnCreature.Model;

namespace MTGCreateYourOwnCreature.View.Converters
{
    public class ManaToSymbolsConverter : IValueConverter
    {
        static Dictionary<MTGCreatureCard.MTGCreatureMana.ManaType, Brush> ms_ManaBruses = new Dictionary<MTGCreatureCard.MTGCreatureMana.ManaType, Brush>()
        {
            { MTGCreatureCard.MTGCreatureMana.ManaType.White, Brushes.White },
            { MTGCreatureCard.MTGCreatureMana.ManaType.Blue, Brushes.Blue },
            { MTGCreatureCard.MTGCreatureMana.ManaType.Black, Brushes.Black },
            { MTGCreatureCard.MTGCreatureMana.ManaType.Red, Brushes.Red },
            { MTGCreatureCard.MTGCreatureMana.ManaType.Green, Brushes.Green },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Dictionary<MTGCreatureCard.MTGCreatureMana.ManaType, int>? manas = value as Dictionary<MTGCreatureCard.MTGCreatureMana.ManaType, int>;
            if (manas == null)
            {
                return null;
            }

            List<MTGManaSymbol> symbols = new List<MTGManaSymbol>();

            foreach (KeyValuePair<MTGCreatureCard.MTGCreatureMana.ManaType, int> pair in manas)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }

                if (pair.Key == MTGCreatureCard.MTGCreatureMana.ManaType.Colorless)
                {
                    symbols.Add(new MTGManaSymbol(pair.Value.ToString(), Brushes.LightGray));
                }
                else
                {
                    symbols.Add(new MTGManaSymbol("", ms_ManaBruses[pair.Key]));
                }
            }

            return symbols;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

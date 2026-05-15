using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;

namespace MTGCreateYourOwnCreature.View.Converters
{
    public class ManaToSymbolsConverter : IValueConverter
    {
        public static Dictionary<ManaType, Brush?> ManaBrushes = new Dictionary<ManaType, Brush?>()
        {
            { ManaType.White, App.Current.TryFindResource("WhiteManaBrush") as Brush },
            { ManaType.Blue, App.Current.TryFindResource("BlueManaBrush") as Brush },
            { ManaType.Black, App.Current.TryFindResource("BlackManaBrush") as Brush },
            { ManaType.Red, App.Current.TryFindResource("RedManaBrush") as Brush },
            { ManaType.Green, App.Current.TryFindResource("GreenManaBrush") as Brush },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Dictionary<ManaType, int>? manas = value as Dictionary<ManaType, int>;
            if (manas == null)
            {
                return null;
            }

            List<MTGManaSymbol> symbols = new List<MTGManaSymbol>();

            foreach (KeyValuePair<ManaType, int> pair in manas)
            {
                if (pair.Value <= 0)
                {
                    continue;
                }

                if (pair.Key == ManaType.Colorless)
                {
                    symbols.Add(new MTGManaSymbol(pair.Value.ToString(), Brushes.LightGray));
                }
                else
                {
                    for (int i = 0; i < pair.Value; ++i)
                    {
                        symbols.Add(new MTGManaSymbol("", ManaBrushes[pair.Key]));
                    }
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

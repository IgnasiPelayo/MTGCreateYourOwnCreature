using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.ViewModel.Cards;

namespace MTGCreateYourOwnCreature.View.Converters
{
    public class ManaToSymbolsConverter : IValueConverter
    {
        public static Dictionary<ManaType, Brush?> ManaBrushes = new Dictionary<ManaType, Brush?>()
        {
            { ManaType.Colorless, Brushes.LightGray },
            { ManaType.White, App.Current.TryFindResource("WhiteManaBrush") as Brush },
            { ManaType.Blue, App.Current.TryFindResource("BlueManaBrush") as Brush },
            { ManaType.Black, App.Current.TryFindResource("BlackManaBrush") as Brush },
            { ManaType.Red, App.Current.TryFindResource("RedManaBrush") as Brush },
            { ManaType.Green, App.Current.TryFindResource("GreenManaBrush") as Brush },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<MTGManaEntryVM> mana = value as List<MTGManaEntryVM>;
            if (mana == null)
            {
                return null;
            }

            List<MTGManaSymbol> symbols = new List<MTGManaSymbol>();
            foreach (MTGManaEntryVM entry in mana)
            {
                if (entry.Value <= 0)
                {
                    continue;
                }

                if (entry.ManaType == ManaType.Colorless)
                {
                    symbols.Add(new MTGManaSymbol(entry.Value.ToString(), ManaBrushes[entry.ManaType]));
                }
                else
                {
                    for (int i = 0; i < entry.Value; ++i)
                    {
                        symbols.Add(new MTGManaSymbol("", ManaBrushes[entry.ManaType]));
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

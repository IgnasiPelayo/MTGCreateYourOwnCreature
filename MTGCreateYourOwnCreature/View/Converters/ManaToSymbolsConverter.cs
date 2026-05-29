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
            { ManaType.Generic, Brushes.LightGray },
            { ManaType.White, App.Current.TryFindResource("WhiteManaBrush") as Brush },
            { ManaType.Blue, App.Current.TryFindResource("BlueManaBrush") as Brush },
            { ManaType.Black, App.Current.TryFindResource("BlackManaBrush") as Brush },
            { ManaType.Red, App.Current.TryFindResource("RedManaBrush") as Brush },
            { ManaType.Green, App.Current.TryFindResource("GreenManaBrush") as Brush },
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IReadOnlyDictionary<ManaType, int> mana = value as IReadOnlyDictionary<ManaType, int>;
            if (mana == null)
            {
                return null;
            }

            List<MTGManaSymbol> symbols = new List<MTGManaSymbol>();
            foreach (KeyValuePair<ManaType, int> manaEntry in mana)
            {
                if (manaEntry.Value == 0)
                {
                    continue;
                }

                if (manaEntry.Key == ManaType.Generic)
                {
                    symbols.Add(new MTGManaSymbol(manaEntry.Value.ToString(), ManaBrushes[manaEntry.Key]));
                }
                else
                {
                    for (int i = 0; i < manaEntry.Value; ++i)
                    {
                        symbols.Add(new MTGManaSymbol("", ManaBrushes[manaEntry.Key]));
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

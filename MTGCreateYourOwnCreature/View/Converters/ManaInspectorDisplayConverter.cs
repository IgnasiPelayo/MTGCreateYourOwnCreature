using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.ViewModel.ItemViewModels;

namespace MTGCreateYourOwnCreature.View.Converters
{
    public class ManaInspectorDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MTGCreatureCard? card = value as MTGCreatureCard;

            List<ManaInspectorDisplayItemVM> items = new List<ManaInspectorDisplayItemVM>();
            if (card != null )
            {
                foreach (ManaType manaType in Enum.GetValues(typeof(ManaType)))
                {
                    Brush brush = manaType == ManaType.Colorless ? Brushes.LightGray : ManaToSymbolsConverter.ManaBrushes[manaType];

                    items.Add(new ManaInspectorDisplayItemVM(card, manaType, brush));
                }
            }

            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

using System.Windows.Data;
using System.Windows.Media;
using System.Globalization;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.ViewModel.ItemViewModels;
using MTGCreateYourOwnCreature.ViewModel.Cards;

namespace MTGCreateYourOwnCreature.View.Converters
{
    public class ManaInspectorDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            MTGCreatureCardVM? cardViewModel = value as MTGCreatureCardVM;

            List<ManaInspectorDisplayItemVM> items = new List<ManaInspectorDisplayItemVM>();
            foreach (ManaType manaType in Enum.GetValues(typeof(ManaType)))
            {
                //items.Add(new ManaInspectorDisplayItemVM(cardViewModel, manaType, ManaToSymbolsConverter.ManaBrushes[manaType]));
            }

            return items;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

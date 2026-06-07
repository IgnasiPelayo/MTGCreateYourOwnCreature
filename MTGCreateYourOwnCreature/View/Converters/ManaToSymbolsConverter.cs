using System.Globalization;
using System.Windows.Data;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Model.Mana;
using MTGCreateYourOwnCreature.Rendering;

namespace MTGCreateYourOwnCreature.View.Converters
{
    /// <summary>
    /// Converts a mana dictionary into the symbol list shown by mana controls.
    /// </summary>
    public class ManaToSymbolsConverter : IValueConverter
    {
        /// <summary>
        /// Converts a mana cost into UI-ready mana symbols.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IReadOnlyDictionary<ManaType, int> mana)
            {
                return Array.Empty<MTGManaSymbol>();
            }

            return ManaRenderService.CreateSymbols(mana);
        }

        /// <summary>
        /// ConvertBack is not needed because mana symbols are display-only.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

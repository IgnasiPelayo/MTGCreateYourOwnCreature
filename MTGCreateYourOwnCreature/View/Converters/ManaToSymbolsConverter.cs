
using System.Globalization;
using System.Windows.Data;

using MTGCreateYourOwnCreature.Model;
using MTGCreateYourOwnCreature.Rendering;
using MTGCreateYourOwnCreature.Model.Mana;

namespace MTGCreateYourOwnCreature.View.Converters
{
    /// <summary>
    /// A WPF value converter that transforms a dictionary of mana costs into a visual collection of <see cref="MTGManaSymbol"/> objects for UI rendering.
    /// </summary>
    public class ManaToSymbolsConverter : IValueConverter
    {
        /// <summary>
        /// Converts a bound dictionary of mana types and their amounts into a UI-ready collection of formatted mana symbols.
        /// </summary>
        /// <param name="value">The dictionary of mana costs produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">An optional converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>A collection of generated <see cref="MTGManaSymbol"/> objects formatted by the <see cref="ManaRenderService"/>, or an empty array if the value is null or invalid.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not IReadOnlyDictionary<ManaType, int> mana)
            {
                return Array.Empty<MTGManaSymbol>();
            }

            return ManaRenderService.CreateSymbols(mana);
        }

        /// <summary>
        /// Not supported. Mana symbol rendering is strictly a one-way visual conversion.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Nothing; this method always throws an exception.</returns>
        /// <exception cref="NotImplementedException">Thrown unconditionally since two-way conversion from visual symbols back to a mana dictionary is not supported.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

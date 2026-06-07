
using System.Windows.Data;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace MTGCreateYourOwnCreature.View.Converters
{
    /// <summary>
    /// A WPF value converter that extracts and returns the human-readable string defined by an enum value's <see cref="DisplayAttribute"/>.
    /// Falls back to the raw enum string name if no attribute is found.
    /// </summary>
    public class EnumDisplayConverter : IValueConverter
    {
        /// <summary>
        /// Converts a bound enum value into a friendly display string for the UI.
        /// </summary>
        /// <param name="value">The enum value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">An optional converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>The localized string from the <see cref="DisplayAttribute"/>, or the raw enum string if the attribute is missing. Returns an empty string if the value is null.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            Type enumType = value.GetType();
            System.Reflection.FieldInfo? enumField = enumType.GetField(value.ToString() ?? "");

            if (enumField != null)
            {
                object[] attributes = enumField.GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attributes.Length > 0)
                {
                    DisplayAttribute display = (DisplayAttribute)attributes[0];
                    return display.Name ?? string.Empty;
                }
            }

            return value.ToString() ?? string.Empty;
        }

        /// <summary>
        /// Not supported. Enum display text is strictly a one-way visual conversion.
        /// </summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>Nothing; this method always throws an exception.</returns>
        /// <exception cref="NotImplementedException">Thrown unconditionally since two-way conversion from localized text back to an enum value is not supported.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

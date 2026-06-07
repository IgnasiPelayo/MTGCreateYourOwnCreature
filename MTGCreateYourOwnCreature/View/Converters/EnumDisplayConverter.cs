using System.Windows.Data;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace MTGCreateYourOwnCreature.View.Converters
{
    /// <summary>
    /// Converts enum values to the friendly names shown in the UI.
    /// </summary>
    public class EnumDisplayConverter : IValueConverter
    {
        /// <summary>
        /// Returns the DisplayAttribute name when the enum value has one.
        /// </summary>
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
        /// ConvertBack is not needed because enum display text is read-only.
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

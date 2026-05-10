using System.Windows.Data;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace MTGCreateYourOwnCreature.View.Converters
{
    public class EnumDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return "";
            }

            Type enumType = value.GetType();
            System.Reflection.FieldInfo? enumField = enumType.GetField(value.ToString() ?? "");

            if (enumField != null)
            {
                object[] attributes = enumField.GetCustomAttributes(typeof(DisplayAttribute), false);
                if (attributes.Length > 0)
                {
                    DisplayAttribute display = (DisplayAttribute)attributes[0];
                    return display.Name;
                }
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

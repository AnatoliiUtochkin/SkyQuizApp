using System.Globalization;
using System.Windows.Data;

namespace SkyQuizApp.Converters
{
    public class DictionaryValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return false;

            var dict = values[0] as IDictionary<int, bool>;
            var key = values[1] as int?;

            if (dict != null && key.HasValue && dict.TryGetValue(key.Value, out bool value))
                return value;

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { Binding.DoNothing, Binding.DoNothing };
        }
    }
}
using System.Globalization;
using System.Windows.Data;

namespace SkyQuizApp.Converters
{
    public class SelectedAnswerEqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return false;

            if (values[0] is int selectedId && values[1] is int thisAnswerId)
                return selectedId == thisAnswerId;

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { Binding.DoNothing, Binding.DoNothing };
        }
    }
}
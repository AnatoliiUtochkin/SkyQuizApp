using System.Globalization;
using System.Windows.Data;
using SkyQuizApp.Models;

namespace SkyQuizApp.Converters
{
    public class TrueFalseTextToAnswerIdConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IEnumerable<Answer> answers && parameter is string label)
            {
                string normalizedLabel = Normalize(label);

                foreach (var answer in answers)
                {
                    if (Normalize(answer.Text) == normalizedLabel)
                        return answer.AnswerID;
                }
            }
            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();

        private string Normalize(string input) =>
            input?.Trim().ToLower().Replace("true", "правда").Replace("false", "неправда");
    }
}
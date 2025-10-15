using System.Globalization;
using System.Windows.Data;
using SkyQuizApp.Enums;

namespace SkyQuizApp.Converters
{
    public class QuestionTypeToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                QuestionType.SingleChoice => "Одна правильна відповідь",
                QuestionType.MultipleChoice => "Кілька правильних відповідей",
                QuestionType.ShortAnswer => "Коротка відповідь",
                QuestionType.TrueFalse => "Так / Ні",
                QuestionType.Matching => "Встановити відповідність",
                _ => "Невідомо"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
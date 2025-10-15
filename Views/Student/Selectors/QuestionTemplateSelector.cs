using System.Windows;
using System.Windows.Controls;
using SkyQuizApp.Enums;
using SkyQuizApp.Models;

namespace SkyQuizApp.Views.Student.Selectors
{
    public class QuestionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? SingleChoiceTemplate { get; set; }
        public DataTemplate? MultipleChoiceTemplate { get; set; }
        public DataTemplate? ShortAnswerTemplate { get; set; }
        public DataTemplate? TrueFalseTemplate { get; set; }
        public DataTemplate? MatchingTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is not Question question)
                return base.SelectTemplate(item, container);

            return question.QuestionType switch
            {
                QuestionType.SingleChoice => SingleChoiceTemplate,
                QuestionType.MultipleChoice => MultipleChoiceTemplate,
                QuestionType.ShortAnswer => ShortAnswerTemplate,
                QuestionType.TrueFalse => TrueFalseTemplate,
                QuestionType.Matching => MatchingTemplate,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
using SkyQuizApp.Models;

namespace SkyQuizApp.ViewModels.Student
{
    public class QuestionNavItem
    {
        public int Index { get; set; }
        public int QuestionID { get; set; }

        public QuestionNavItem(int index, Question question)
        {
            Index = index;
            QuestionID = question.QuestionID;
        }
    }
}
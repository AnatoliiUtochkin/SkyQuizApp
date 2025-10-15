using System.Windows;

namespace SkyQuizApp.Views.Student
{
    public partial class TestResultWindow : Window
    {
        public string ScorePercentText { get; set; } = string.Empty;
        public string Score12Text { get; set; } = string.Empty;
        public string LetterGradeText { get; set; } = string.Empty;

        public TestResultWindow(decimal score)
        {
            InitializeComponent();

            ScorePercentText = $"Успішність: {score:F1}%";
            Score12Text = $"Оцінка (12-бальна): {Math.Round(score / 100m * 12m, 1):F1}";
            LetterGradeText = $"Оцінка (LGA): {GetLetterGrade(score)}";

            DataContext = this;
        }

        private string GetLetterGrade(decimal score)
        {
            if (score >= 97) return "A+";
            if (score >= 93) return "A";
            if (score >= 90) return "A−";
            if (score >= 87) return "B+";
            if (score >= 83) return "B";
            if (score >= 80) return "B−";
            if (score >= 77) return "C+";
            if (score >= 73) return "C";
            if (score >= 70) return "C−";
            if (score >= 67) return "D+";
            if (score >= 63) return "D";
            if (score >= 60) return "D−";
            return "F";
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
namespace SkyQuizApp.Dto
{
    public class StudentTestInfoDto
    {
        public int? TestID { get; set; }
        public string TestTitle { get; set; } = string.Empty;
        public int AttemptsCount { get; set; }
        public double AverageScore { get; set; }

        public string SummaryInfo => $"Спроб: {AttemptsCount} | Середній результат: {AverageScore:F1}%";
    }
}
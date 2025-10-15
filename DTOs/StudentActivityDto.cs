namespace SkyQuizApp.DTOs
{
    public class StudentActivityDto
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int TestCount { get; set; }
        public decimal AverageScore { get; set; }
    }
}
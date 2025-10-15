namespace SkyQuizApp.DTOs
{
    public class StudentResultDto
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
        public string Score { get; set; } = string.Empty;
        public string Grade12 { get; set; } = string.Empty;
        public string Grade100 { get; set; } = string.Empty;
    }
}
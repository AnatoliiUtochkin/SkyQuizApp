using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyQuizApp.Models
{
    public class Test
    {
        [Key]
        [Display(Name = "Test ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TestID { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [Display(Name = "Title")]
        [StringLength(50, ErrorMessage = "Title cannot exceed 50 characters.")]
        public string Title { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [StringLength(100, ErrorMessage = "Description cannot exceed 100 characters.")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Test Key")]
        [StringLength(6, ErrorMessage = "String Length cannot exceed 6 characters.")]
        public string TestKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "User ID is required.")]
        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Creation date is required.")]
        [Display(Name = "Created At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Time Limit (Minutes)")]
        [Range(1, 300, ErrorMessage = "Time limit must be between 1 and 300 minutes.")]
        public int TimeLimitMinutes { get; set; }

        [Display(Name = "Shuffle Questions")]
        public bool ShuffleQuestions { get; set; } = false;

        [Display(Name = "Deadline")]
        public DateTime? Deadline { get; set; } = null;

        [Display(Name = "Attempts limit")]
        [Range(1, 5, ErrorMessage = "Attempts limit must be between 1 and 5.")]
        public int AttemptsLimit { get; set; } = 0;

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        public Test()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public ICollection<TestSession> TestSessions { get; set; } = new List<TestSession>();
    }
}
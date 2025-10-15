using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyQuizApp.Models
{
    public class UserAnswer
    {
        [Key]
        [Display(Name = "User Answer ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserAnswerID { get; set; }

        [Required(ErrorMessage = "Session ID is required.")]
        [Display(Name = "Session ID")]
        public int TestSessionID { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Question ID is required.")]
        [Display(Name = "Question ID")]
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "Answer ID is required.")]
        [Display(Name = "Answer ID")]
        public int AnswerID { get; set; }

        [Required(ErrorMessage = "Correctness status is required.")]
        public bool IsCorrect { get; set; }

        [Required(ErrorMessage = "Answered At is required.")]
        [Display(Name = "Answered At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime AnsweredAt { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("QuestionID")]
        public virtual Question? Question { get; set; }

        [ForeignKey("AnswerID")]
        public virtual Answer? Answer { get; set; }

        [ForeignKey("TestSessionID")]
        public virtual TestSession? TestSession { get; set; }

        public UserAnswer()
        {
            AnsweredAt = DateTime.UtcNow;
        }
    }
}
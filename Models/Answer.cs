using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyQuizApp.Models
{
    public class Answer
    {
        [Key]
        [Display(Name = "Answer ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AnswerID { get; set; }

        [Required(ErrorMessage = "Question ID is required.")]
        [Display(Name = "Question ID")]
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "Answer text is required.")]
        [StringLength(200, ErrorMessage = "Answer text cannot exceed 200 characters.")]
        [Display(Name = "Answer Text")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Correctness status is required.")]
        [Display(Name = "Is Correct")]
        public bool IsCorrect { get; set; } = false;

        [ForeignKey("QuestionID")]
        public virtual Question? Question { get; set; }
    }
}
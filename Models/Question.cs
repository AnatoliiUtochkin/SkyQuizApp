using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SkyQuizApp.Enums;

namespace SkyQuizApp.Models
{
    public class Question
    {
        [Key]
        [Display(Name = "Question ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int QuestionID { get; set; }

        [Required(ErrorMessage = "Test ID is required.")]
        [Display(Name = "Test ID")]
        public int TestID { get; set; }

        [Required(ErrorMessage = "Question text is required.")]
        [StringLength(500, ErrorMessage = "Question text cannot exceed 500 characters.")]
        [Display(Name = "Question Text")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Question type is required.")]
        [EnumDataType(typeof(QuestionType), ErrorMessage = "Invalid question type.")]
        [Display(Name = "Question Type")]
        public QuestionType QuestionType { get; set; } = QuestionType.SingleChoice;

        [Required(ErrorMessage = "Creation date is required.")]
        [Display(Name = "Created At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [ForeignKey("TestID")]
        public virtual Test? Test { get; set; }

        public Question()
        {
            CreatedAt = DateTime.UtcNow;
        }

        public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
    }
}
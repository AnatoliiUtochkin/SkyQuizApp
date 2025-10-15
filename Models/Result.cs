using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyQuizApp.Models
{
    public class Result
    {
        [Key]
        [Display(Name = "Result ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResultID { get; set; }

        [Required(ErrorMessage = "Session ID is required.")]
        [Display(Name = "Session ID")]
        public int SessionID { get; set; }

        [Required(ErrorMessage = "Score is required.")]
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100.")]
        [Display(Name = "Score")]
        public decimal Score { get; set; }

        [ForeignKey("SessionID")]
        public virtual TestSession? TestSession { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyQuizApp.Models
{
    public class TestSession
    {
        [Key]
        [Display(Name = "Session ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TestSessionID { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Test ID is required.")]
        [Display(Name = "Test ID")]
        public int? TestID { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        [Display(Name = "Started At")]
        public DateTime StartedAt { get; set; }

        [Required(ErrorMessage = "Complete date is required.")]
        [Display(Name = "Completed At")]
        public DateTime CompletedAt { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("TestID")]
        public virtual Test? Test { get; set; }

        public virtual Result? Result { get; set; }
    }
}
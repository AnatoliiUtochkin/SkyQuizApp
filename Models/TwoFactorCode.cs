using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkyQuizApp.Models
{
    public class TwoFactorCode
    {
        [Key]
        [Display(Name = "Code ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CodeID { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Code is required.")]
        [MaxLength(10, ErrorMessage = "Code cannot exceed 10 characters.")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiration time is required.")]
        [Display(Name = "Expires At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime ExpiresAt { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
    }
}
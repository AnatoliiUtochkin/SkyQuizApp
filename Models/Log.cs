using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SkyQuizApp.Enums;

namespace SkyQuizApp.Models
{
    public class Log
    {
        [Key]
        [Display(Name = "Log ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LogID { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        [Display(Name = "User ID")]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Action is required.")]
        [EnumDataType(typeof(LogAction), ErrorMessage = "Invalid action.")]
        [Display(Name = "Action")]
        public LogAction Action { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        [Display(Name = "Timestamp")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime Timestamp { get; set; }

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        public Log()
        {
            Timestamp = DateTime.UtcNow;
        }
    }
}
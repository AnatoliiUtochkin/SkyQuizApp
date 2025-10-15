using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SkyQuizApp.Enums;

namespace SkyQuizApp.Models
{
    public class User
    {
        [Key]
        [Display(Name = "User ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")] 
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [MaxLength(100, ErrorMessage = "Full Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
        [Display(Name = "Hashed Password")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "User role is required.")]
        [EnumDataType(typeof(UserRole), ErrorMessage = "Invalid user role.")]
        [Display(Name = "User Role")]
        public UserRole Role { get; set; }

        [Display(Name = "Is Blocked")]
        public bool IsBlocked { get; set; } = false;

        [Display(Name = "Two-Factor Enabled")]
        public bool IsTwoFactorEnabled { get; set; } = false;

        [Required(ErrorMessage = "Created At is required.")]
        [Display(Name = "Created At")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Last Login")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm:ss}")]
        public DateTime? LastLogin { get; set; }

        public User()
        {
            CreatedAt = DateTime.UtcNow;
        }
    }
}
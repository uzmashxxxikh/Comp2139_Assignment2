using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Contact Information")]
        public string ContactInformation { get; set; } = string.Empty;

        [Display(Name = "Preferred Categories")]
        public string? PreferredCategories { get; set; }

        [Display(Name = "Email Confirmation Token")]
        public string? EmailConfirmationToken { get; set; }

        [Display(Name = "Email Confirmation Token Expiry")]
        public DateTime? EmailConfirmationTokenExpiry { get; set; }
    }
} 
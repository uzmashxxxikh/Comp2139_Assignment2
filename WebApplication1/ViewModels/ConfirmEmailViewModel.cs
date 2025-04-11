using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class ConfirmEmailViewModel
    {
        [Required]
        public string StatusMessage { get; set; }
    }
} 
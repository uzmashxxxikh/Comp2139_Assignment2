using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels
{
    public class ProductSearchViewModel
    {
        [Display(Name = "Product Name")]
        public string? Name { get; set; }

        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Minimum Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Minimum price must be greater than or equal to 0")]
        public decimal? MinPrice { get; set; }

        [Display(Name = "Maximum Price")]
        [Range(0, double.MaxValue, ErrorMessage = "Maximum price must be greater than or equal to 0")]
        public decimal? MaxPrice { get; set; }

        [Display(Name = "Low Stock Only")]
        public bool LowStock { get; set; }
    }
} 
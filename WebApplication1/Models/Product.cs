using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication1.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [Column(TypeName = "decimal(18,2)")]
        [CustomValidation(typeof(Product), nameof(ValidatePrice))]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity in stock cannot be negative")]
        [CustomValidation(typeof(Product), nameof(ValidateQuantityInStock))]
        public int QuantityInStock { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Low stock threshold cannot be negative")]
        [CustomValidation(typeof(Product), nameof(ValidateLowStockThreshold))]
        public int LowStockThreshold { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Category ID must be greater than 0")]
        [CustomValidation(typeof(Product), nameof(ValidateCategoryId))]
        public int CategoryId { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public static ValidationResult? ValidatePrice(decimal value, ValidationContext validationContext)
        {
            if (value <= 0)
            {
                return new ValidationResult("Price must be greater than 0");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateQuantityInStock(int value, ValidationContext validationContext)
        {
            if (value < 0)
            {
                return new ValidationResult("Quantity in stock cannot be negative");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateLowStockThreshold(int value, ValidationContext validationContext)
        {
            if (value < 0)
            {
                return new ValidationResult("Low stock threshold cannot be negative");
            }
            return ValidationResult.Success;
        }

        public static ValidationResult? ValidateCategoryId(int value, ValidationContext validationContext)
        {
            if (value <= 0)
            {
                return new ValidationResult("Category ID must be greater than 0");
            }
            return ValidationResult.Success;
        }
    }
} 
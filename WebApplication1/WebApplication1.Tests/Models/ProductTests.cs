using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Models
{
    public class ProductTests
    {
        [Fact]
        public void Product_WithValidData_IsValid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00m,
                QuantityInStock = 5,
                LowStockThreshold = 2,
                CategoryId = 1
            };

            // Act
            var validationContext = new ValidationContext(product);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Product_WithEmptyName_IsInvalid()
        {
            // Arrange
            var product = new Product
            {
                Name = "",
                Description = "Test Description",
                Price = 10.00m,
                QuantityInStock = 5,
                LowStockThreshold = 2,
                CategoryId = 1
            };

            // Act
            var validationContext = new ValidationContext(product);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Name"));
        }

        [Fact]
        public void Product_WithLongDescription_IsInvalid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = new string('a', 1001), // Exceeds StringLength(1000)
                Price = 10.00m,
                QuantityInStock = 5,
                LowStockThreshold = 2,
                CategoryId = 1
            };

            // Act
            var validationContext = new ValidationContext(product);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Description"));
        }

        [Fact]
        public void Product_WithNegativePrice_IsInvalid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = -10.00m,
                QuantityInStock = 5,
                LowStockThreshold = 2,
                CategoryId = 1
            };

            // Act
            var validationContext = new ValidationContext(product);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("Price"));
        }

        [Fact]
        public void Product_WithNegativeQuantityInStock_IsInvalid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00m,
                QuantityInStock = -5,
                LowStockThreshold = 2,
                CategoryId = 1
            };

            // Act
            var validationContext = new ValidationContext(product);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("QuantityInStock"));
        }

        [Fact]
        public void Product_WithNegativeLowStockThreshold_IsInvalid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00m,
                QuantityInStock = 5,
                LowStockThreshold = -2,
                CategoryId = 1
            };

            // Act
            var validationContext = new ValidationContext(product);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("LowStockThreshold"));
        }

        [Fact]
        public void Product_WithZeroCategoryId_IsInvalid()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00m,
                QuantityInStock = 5,
                LowStockThreshold = 2,
                CategoryId = 0
            };

            // Act
            var validationContext = new ValidationContext(product);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(product, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("CategoryId"));
        }

        [Fact]
        public void Product_WithDefaultValues_HasEmptyCollections()
        {
            // Arrange & Act
            var product = new Product();

            // Assert
            Assert.NotNull(product.OrderItems);
            Assert.Empty(product.OrderItems);
            Assert.Equal(string.Empty, product.Name);
            Assert.Null(product.Description);
        }

        [Fact]
        public void Product_CanAddOrderItems()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00m,
                QuantityInStock = 5,
                LowStockThreshold = 2,
                CategoryId = 1
            };

            var orderItem = new OrderItem
            {
                OrderId = 1,
                Quantity = 2,
                UnitPrice = 10.00m
            };

            // Act
            product.OrderItems.Add(orderItem);

            // Assert
            Assert.Single(product.OrderItems);
            Assert.Equal(orderItem, product.OrderItems.First());
        }

        [Fact]
        public void Product_IsLowStock_WhenQuantityBelowThreshold()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00m,
                QuantityInStock = 1,
                LowStockThreshold = 2,
                CategoryId = 1
            };

            // Act
            var isLowStock = product.QuantityInStock <= product.LowStockThreshold;

            // Assert
            Assert.True(isLowStock);
        }

        [Fact]
        public void Product_IsNotLowStock_WhenQuantityAboveThreshold()
        {
            // Arrange
            var product = new Product
            {
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00m,
                QuantityInStock = 5,
                LowStockThreshold = 2,
                CategoryId = 1
            };

            // Act
            var isLowStock = product.QuantityInStock <= product.LowStockThreshold;

            // Assert
            Assert.False(isLowStock);
        }
    }
}
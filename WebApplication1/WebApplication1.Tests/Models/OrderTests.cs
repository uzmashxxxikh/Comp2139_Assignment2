using System.ComponentModel.DataAnnotations;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Models
{
    public class OrderTests
    {
        [Fact]
        public void Order_WithValidData_IsValid()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "John Doe",
                GuestEmail = "john@example.com",
                OrderDate = DateTime.Now,
                TotalAmount = 100.00m,
                OrderItems = new List<OrderItem>()
            };

            // Act
            var validationContext = new ValidationContext(order);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Order_WithEmptyGuestName_IsInvalid()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "",
                GuestEmail = "john@example.com",
                OrderDate = DateTime.Now,
                TotalAmount = 100.00m,
                OrderItems = new List<OrderItem>()
            };

            // Act
            var validationContext = new ValidationContext(order);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("GuestName"));
        }

        [Fact]
        public void Order_WithInvalidEmail_IsInvalid()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "John Doe",
                GuestEmail = "invalid-email",
                OrderDate = DateTime.Now,
                TotalAmount = 100.00m,
                OrderItems = new List<OrderItem>()
            };

            // Act
            var validationContext = new ValidationContext(order);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("GuestEmail"));
        }

        [Fact]
        public void Order_WithNegativeTotalAmount_IsInvalid()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "John Doe",
                GuestEmail = "john@example.com",
                OrderDate = DateTime.Now,
                TotalAmount = -100.00m,
                OrderItems = new List<OrderItem>()
            };

            // Act
            var validationContext = new ValidationContext(order);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("TotalAmount"));
        }

        [Fact]
        public void Order_WithOrderItems_CalculatesTotalAmount()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "John Doe",
                GuestEmail = "john@example.com",
                OrderDate = DateTime.Now,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Quantity = 2, UnitPrice = 10.00m },
                    new OrderItem { Quantity = 1, UnitPrice = 20.00m }
                }
            };

            // Act
            order.TotalAmount = order.OrderItems.Sum(item => item.Quantity * item.UnitPrice);

            // Assert
            Assert.Equal(40.00m, order.TotalAmount);
        }

        [Fact]
        public void Order_WithLongGuestName_IsInvalid()
        {
            // Arrange
            var order = new Order
            {
                GuestName = new string('a', 101), // Exceeds StringLength(100)
                GuestEmail = "john@example.com",
                OrderDate = DateTime.Now,
                TotalAmount = 100.00m,
                OrderItems = new List<OrderItem>()
            };

            // Act
            var validationContext = new ValidationContext(order);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("GuestName"));
        }

        [Fact]
        public void Order_WithLongEmail_IsInvalid()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "John Doe",
                GuestEmail = new string('a', 90) + "@example.com", // Exceeds StringLength(100)
                OrderDate = DateTime.Now,
                TotalAmount = 100.00m,
                OrderItems = new List<OrderItem>()
            };

            // Act
            var validationContext = new ValidationContext(order);
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(order, validationContext, validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, v => v.MemberNames.Contains("GuestEmail"));
        }

        [Fact]
        public void Order_WithDefaultValues_HasEmptyCollections()
        {
            // Arrange & Act
            var order = new Order();

            // Assert
            Assert.NotNull(order.OrderItems);
            Assert.Empty(order.OrderItems);
            Assert.Equal(string.Empty, order.GuestName);
            Assert.Equal(string.Empty, order.GuestEmail);
        }

        [Fact]
        public void Order_CanAddOrderItems()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "John Doe",
                GuestEmail = "john@example.com",
                OrderDate = DateTime.Now,
                TotalAmount = 100.00m
            };

            var orderItem = new OrderItem
            {
                ProductId = 1,
                Quantity = 2,
                UnitPrice = 10.00m
            };

            // Act
            order.OrderItems.Add(orderItem);

            // Assert
            Assert.Single(order.OrderItems);
            Assert.Equal(orderItem, order.OrderItems.First());
        }
    }
}
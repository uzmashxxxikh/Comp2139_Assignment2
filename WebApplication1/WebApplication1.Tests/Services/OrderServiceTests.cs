using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using Xunit;

namespace WebApplication1.Tests.Services
{
    public class OrderServiceTests
    {
        private readonly Mock<ILogger<OrderService>> _loggerMock;
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            // Set up in-memory database
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);
            _loggerMock = new Mock<ILogger<OrderService>>();
            _orderService = new OrderService(_context, _loggerMock.Object);

            // Seed the database
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var category = new Category { Id = 1, Name = "Test Category" };
            _context.Categories.Add(category);

            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Description = "Test Description",
                Price = 10.00m,
                QuantityInStock = 10,
                LowStockThreshold = 2,
                CategoryId = 1,
                Category = category
            };
            _context.Products.Add(product);

            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateOrderAsync_WithValidOrder_CreatesOrderAndUpdatesStock()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "Test User",
                GuestEmail = "test@example.com",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        Quantity = 2
                    }
                }
            };

            // Act
            var result = await _orderService.CreateOrderAsync(order);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(20.00m, result.TotalAmount); // 2 items * $10.00
            Assert.Equal(DateTimeKind.Utc, result.OrderDate.Kind);

            // Verify stock was updated
            var product = await _context.Products.FindAsync(1);
            Assert.Equal(8, product.QuantityInStock); // 10 - 2
        }

        [Fact]
        public async Task CreateOrderAsync_WithInsufficientStock_ThrowsException()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "Test User",
                GuestEmail = "test@example.com",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        Quantity = 15 // More than available stock
                    }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                _orderService.CreateOrderAsync(order));
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithExistingOrder_ReturnsOrder()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "Test User",
                GuestEmail = "test@example.com",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem
                    {
                        ProductId = 1,
                        Quantity = 1
                    }
                }
            };
            await _orderService.CreateOrderAsync(order);

            // Act
            var result = await _orderService.GetOrderByIdAsync(order.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(order.GuestEmail, result.GuestEmail);
            Assert.Single(result.OrderItems);
        }

        [Fact]
        public async Task GetOrderByIdAsync_WithNonExistingOrder_ReturnsNull()
        {
            // Act
            var result = await _orderService.GetOrderByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetOrdersByEmailAsync_ReturnsMatchingOrders()
        {
            // Arrange
            var email = "test@example.com";
            var order1 = new Order
            {
                GuestName = "Test User 1",
                GuestEmail = email,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1 }
                }
            };
            var order2 = new Order
            {
                GuestName = "Test User 2",
                GuestEmail = email,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1 }
                }
            };
            await _orderService.CreateOrderAsync(order1);
            await _orderService.CreateOrderAsync(order2);

            // Act
            var results = await _orderService.GetOrdersByEmailAsync(email);

            // Assert
            Assert.Equal(2, results.Count);
            Assert.All(results, o => Assert.Equal(email, o.GuestEmail));
        }

        [Fact]
        public async Task UpdateOrderAsync_WithValidOrder_UpdatesSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "Test User",
                GuestEmail = "test@example.com",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1 }
                }
            };
            await _orderService.CreateOrderAsync(order);

            // Act
            order.GuestName = "Updated Name";
            var result = await _orderService.UpdateOrderAsync(order);

            // Assert
            Assert.True(result);
            var updatedOrder = await _orderService.GetOrderByIdAsync(order.Id);
            Assert.Equal("Updated Name", updatedOrder.GuestName);
        }

        [Fact]
        public async Task DeleteOrderAsync_WithExistingOrder_DeletesSuccessfully()
        {
            // Arrange
            var order = new Order
            {
                GuestName = "Test User",
                GuestEmail = "test@example.com",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1 }
                }
            };
            await _orderService.CreateOrderAsync(order);

            // Act
            var result = await _orderService.DeleteOrderAsync(order.Id);

            // Assert
            Assert.True(result);
            var deletedOrder = await _orderService.GetOrderByIdAsync(order.Id);
            Assert.Null(deletedOrder);
        }

        [Fact]
        public async Task GetAllOrdersAsync_ReturnsAllOrders()
        {
            // Arrange
            var order1 = new Order
            {
                GuestName = "Test User 1",
                GuestEmail = "test1@example.com",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1 }
                }
            };
            var order2 = new Order
            {
                GuestName = "Test User 2",
                GuestEmail = "test2@example.com",
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 1, Quantity = 1 }
                }
            };
            await _orderService.CreateOrderAsync(order1);
            await _orderService.CreateOrderAsync(order2);

            // Act
            var results = await _orderService.GetAllOrdersAsync();

            // Assert
            Assert.Equal(2, results.Count);
        }
    }
} 
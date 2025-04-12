using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Models;
using Xunit;

namespace WebApplication1.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB_" + Guid.NewGuid())
                .Options;

            _context = new ApplicationDbContext(options);

            var loggerMock = new Mock<ILogger<ProductController>>();
            _controller = new ProductController(_context, loggerMock.Object);

            var httpContext = new DefaultHttpContext();
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "admin@test.com"),
                new Claim(ClaimTypes.Role, "Admin")
            };
            httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Create_WithValidProduct_RedirectsToIndex()
        {
            var category = new Category { Name = "Test Category" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Test Product",
                Description = "A test description",
                Price = 25.99M,
                QuantityInStock = 100,
                LowStockThreshold = 5,
                CategoryId = category.Id
            };

            var result = await _controller.Create(product);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
        }

        [Fact]
        public async Task DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            var category = new Category { Name = "Test Category" };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Name = "Product to Delete",
                Description = "To be deleted",
                Price = 15.00M,
                QuantityInStock = 10,
                LowStockThreshold = 2,
                CategoryId = category.Id
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteConfirmed(product.Id);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.False(_context.Products.Any(p => p.Id == product.Id));
        }
    }
}

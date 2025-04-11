using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using WebApplication1.Controllers;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services;
using Xunit;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http.Features;

namespace WebApplication1.Tests;

public class OrderControllerTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly Mock<ILogger<OrderController>> _loggerMock;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        // Create in-memory database options
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        // Create real context with in-memory database
        _context = new ApplicationDbContext(options);

        // Create mocks for other dependencies
        _orderServiceMock = new Mock<IOrderService>();
        _loggerMock = new Mock<ILogger<OrderController>>();

        // Create controller with real context and mocked dependencies
        _controller = new OrderController(_context, _orderServiceMock.Object, _loggerMock.Object);

        // Set up HttpContext
        var httpContext = new DefaultHttpContext();
        
        // Set up TempData
        var tempDataProvider = new Mock<ITempDataProvider>();
        var tempDataDictionary = new TempDataDictionary(httpContext, tempDataProvider.Object);
        _controller.TempData = tempDataDictionary;

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task Index_ReturnsViewWithOrders()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 10.00m, QuantityInStock = 5 };
        await _context.Products.AddAsync(product);

        var order = new Order
        {
            Id = 1,
            GuestName = "Test Guest",
            GuestEmail = "test@example.com",
            OrderDate = DateTime.Now,
            TotalAmount = 20.00m,
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 10.00m }
            }
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Set up user identity with Admin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext.User = principal;

        // Act
        var result = await _controller.Index();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Order>>(viewResult.Model);
        Assert.Single(model);
    }

    [Fact]
    public void Create_ReturnsViewWithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.00m },
            new Product { Id = 2, Name = "Product 2", Price = 20.00m }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();

        // Act
        var result = _controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.ViewData["Products"]);
    }

    [Fact]
    public async Task Create_WithValidOrder_RedirectsToTrack()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 10.00m, QuantityInStock = 5 };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            GuestName = "Test Guest",
            GuestEmail = "test@example.com",
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 2 }
            }
        };

        _orderServiceMock.Setup(s => s.CreateOrderAsync(It.IsAny<Order>()))
            .ReturnsAsync(new Order { Id = 1 });

        // Set up user identity with Admin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext.User = principal;

        // Set up form with all required values
        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "__RequestVerificationToken", "test-token" },
            { "GuestName", "Test Guest" },
            { "GuestEmail", "test@example.com" }
        });

        _controller.ControllerContext.HttpContext.Request.Form = formCollection;
        _controller.ControllerContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";

        // Act
        var result = await _controller.Create(order);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(OrderController.Track), redirectResult.ActionName);
        Assert.Equal(1, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task Create_WithInvalidOrder_ReturnsViewWithModel()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Price = 10.00m },
            new Product { Id = 2, Name = "Product 2", Price = 20.00m }
        };

        _context.Products.AddRange(products);
        await _context.SaveChangesAsync();

        var order = new Order
        {
            GuestName = "John Doe",
            GuestEmail = "john@example.com",
            OrderItems = new List<OrderItem>()
        };

        _controller.ModelState.AddModelError("OrderItems", "At least one order item is required");

        // Act
        var result = await _controller.Create(order);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Order>(viewResult.Model);
        Assert.Equal("John Doe", model.GuestName);
        Assert.NotNull(viewResult.ViewData["Products"]);
    }

    [Fact]
    public async Task Details_WithValidId_ReturnsViewWithOrder()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "Test Product", Price = 10.00m, QuantityInStock = 5 };
        await _context.Products.AddAsync(product);

        var order = new Order
        {
            Id = 1,
            GuestName = "Test Guest",
            GuestEmail = "test@example.com",
            OrderDate = DateTime.Now,
            TotalAmount = 20.00m,
            OrderItems = new List<OrderItem>
            {
                new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 10.00m }
            }
        };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Set up user identity with Admin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext.HttpContext.User = principal;

        // Act
        var result = await _controller.Details(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Order>(viewResult.Model);
        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task Details_WithNullId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Details(null);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.Details(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_WithValidId_ReturnsViewWithOrder()
    {
        // Arrange
        var order = new Order { Id = 1, GuestName = "John Doe", GuestEmail = "john@example.com" };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Edit(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Order>(viewResult.Model);
        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task Edit_WithValidModel_RedirectsToIndex()
    {
        // Arrange
        var order = new Order { Id = 1, GuestName = "John Doe", GuestEmail = "john@example.com" };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Edit(1, order);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(OrderController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Delete_WithValidId_ReturnsViewWithOrder()
    {
        // Arrange
        var order = new Order { Id = 1, GuestName = "John Doe", GuestEmail = "john@example.com" };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Delete(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Order>(viewResult.Model);
        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task DeleteConfirmed_WithValidId_RedirectsToIndex()
    {
        // Arrange
        var order = new Order { Id = 1, GuestName = "John Doe", GuestEmail = "john@example.com" };
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteConfirmed(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(OrderController.Index), redirectResult.ActionName);
    }

    [Fact]
    public void Track_ReturnsView()
    {
        // Act
        var result = _controller.Track();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task TrackById_WithValidId_ReturnsViewWithOrder()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            GuestName = "Test Guest",
            GuestEmail = "test@example.com",
            OrderDate = DateTime.Now,
            TotalAmount = 20.00m
        };

        _orderServiceMock.Setup(s => s.GetOrderByIdAsync(1))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.TrackById(1);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Track", viewResult.ViewName);
        var model = Assert.IsType<Order>(viewResult.Model);
        Assert.Equal(1, model.Id);
    }

    [Fact]
    public async Task TrackById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _orderServiceMock.Setup(s => s.GetOrderByIdAsync(999))
            .ReturnsAsync((Order)null);

        // Act
        var result = await _controller.TrackById(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void TrackByEmail_ReturnsView()
    {
        // Act
        var result = _controller.TrackByEmail();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task TrackByEmail_WithValidEmail_ReturnsViewWithOrders()
    {
        // Arrange
        var orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                GuestName = "Test Guest",
                GuestEmail = "test@example.com",
                OrderDate = DateTime.Now,
                TotalAmount = 20.00m
            }
        };

        _orderServiceMock.Setup(s => s.GetOrdersByEmailAsync("test@example.com"))
            .ReturnsAsync(orders);

        // Set up form with email
        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "__RequestVerificationToken", "test-token" },
            { "email", "test@example.com" }
        });

        _controller.ControllerContext.HttpContext.Request.Form = formCollection;
        _controller.ControllerContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";

        // Act
        var result = await _controller.TrackByEmail("test@example.com");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("TrackList", viewResult.ViewName);
        var model = Assert.IsAssignableFrom<IEnumerable<Order>>(viewResult.Model);
        Assert.Single(model);
    }

    [Fact]
    public async Task TrackByEmail_WithEmptyEmail_ReturnsViewWithError()
    {
        // Act
        var result = await _controller.TrackByEmail("");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(_controller.ModelState.ContainsKey(""));
    }

    [Fact]
    public async Task DeleteOrder_WithValidId_RedirectsToTrack()
    {
        // Arrange
        _orderServiceMock.Setup(s => s.DeleteOrderAsync(1))
            .ReturnsAsync(true);

        // Set up form with anti-forgery token
        var formCollection = new FormCollection(new Dictionary<string, StringValues>
        {
            { "__RequestVerificationToken", "test-token" }
        });

        _controller.ControllerContext.HttpContext.Request.Form = formCollection;
        _controller.ControllerContext.HttpContext.Request.ContentType = "application/x-www-form-urlencoded";

        // Act
        var result = await _controller.DeleteOrder(1);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(OrderController.Track), redirectResult.ActionName);
    }

    [Fact]
    public async Task DeleteOrder_WithInvalidId_RedirectsToTrack()
    {
        // Arrange
        _orderServiceMock.Setup(s => s.DeleteOrderAsync(999))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteOrder(999);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Track", redirectResult.ActionName);
    }
}
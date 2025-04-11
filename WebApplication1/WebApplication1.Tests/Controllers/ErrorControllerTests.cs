using Microsoft.AspNetCore.Mvc;
using WebApplication1.Controllers;
using Xunit;

namespace WebApplication1.Tests;

public class ErrorControllerTests
{
    private readonly ErrorController _controller;

    public ErrorControllerTests()
    {
        _controller = new ErrorController();
    }

    [Fact]
    public void Error404_ReturnsCorrectView()
    {
        // Act
        var result = _controller.Error404();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Shared/Error404.cshtml", viewResult.ViewName);
    }

    [Fact]
    public void Error500_ReturnsCorrectView()
    {
        // Act
        var result = _controller.Error500();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("~/Views/Shared/Error500.cshtml", viewResult.ViewName);
    }

    [Theory]
    [InlineData(404, "~/Views/Shared/Error404.cshtml")]
    [InlineData(500, "~/Views/Shared/Error500.cshtml")]
    [InlineData(403, "~/Views/Shared/Error500.cshtml")] // Default case
    [InlineData(401, "~/Views/Shared/Error500.cshtml")] // Default case
    public void Error_WithStatusCode_ReturnsCorrectView(int statusCode, string expectedViewName)
    {
        // Act
        var result = _controller.Error(statusCode);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedViewName, viewResult.ViewName);
    }
}
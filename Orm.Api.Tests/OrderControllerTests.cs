using Microsoft.AspNetCore.Mvc;
using Moq;
using Orm.Api.Controllers;
using Orm.Application.Dtos;
using Orm.Application.Services;

namespace Orm.Api.Tests;

public class OrderControllerTests
{
    private readonly Mock<IOrderService> _serviceMock = new();
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _controller = new OrderController(_serviceMock.Object);
    }

    [Fact]
    public async Task GetOrderAsync_Returns200WithDto_WhenFound()
    {
        var dto = new OrderDto { OrderID = 1, CustomerName = "Alice" };
        _serviceMock.Setup(s => s.GetOrderByIdAsync(1)).ReturnsAsync(dto);

        var result = await _controller.GetOrderAsync(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task GetOrderAsync_Returns404_WhenNotFound()
    {
        _serviceMock.Setup(s => s.GetOrderByIdAsync(999)).ReturnsAsync((OrderDto?)null);

        var result = await _controller.GetOrderAsync(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_Returns201Created_WithLocationHeader()
    {
        var createDto = new CreateOrderDto { CustomerName = "Bob" };
        var resultDto = new OrderDto { OrderID = 42, CustomerName = "Bob" };
        _serviceMock.Setup(s => s.CreateOrderAsync(createDto)).ReturnsAsync(resultDto);

        var result = await _controller.CreateOrderAsync(createDto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(resultDto, createdResult.Value);
        Assert.Equal("GetOrder", createdResult.ActionName);
        Assert.Equal(42L, createdResult.RouteValues!["id"]);
    }

    [Fact]
    public async Task UpdateOrderAsync_Returns400_WhenIdMismatch()
    {
        var updateDto = new UpdateOrderDto { OrderID = 5 };

        var result = await _controller.UpdateOrderAsync(updateDto, 10);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateOrderAsync_Returns400_WhenIdIsZeroOrNegative()
    {
        var updateDto = new UpdateOrderDto { OrderID = 0 };

        var result = await _controller.UpdateOrderAsync(updateDto, 0);

        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateOrderAsync_Returns200_WithUpdatedDto()
    {
        var updateDto = new UpdateOrderDto { OrderID = 3, CustomerName = "Carol" };
        var resultDto = new OrderDto { OrderID = 3, CustomerName = "Carol" };
        _serviceMock.Setup(s => s.UpdateOrderAsync(updateDto)).ReturnsAsync(resultDto);

        var result = await _controller.UpdateOrderAsync(updateDto, 3);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(resultDto, okResult.Value);
    }

    [Fact]
    public async Task DeleteOrderAsync_Returns200_OnSuccess()
    {
        _serviceMock.Setup(s => s.DeleteOrderAsync(1)).Returns(Task.CompletedTask);

        var result = await _controller.DeleteOrderAsync(1);

        Assert.IsType<OkResult>(result);
        _serviceMock.Verify(s => s.DeleteOrderAsync(1), Times.Once);
    }
}

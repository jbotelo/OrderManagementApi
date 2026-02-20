using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Orm.Api.Controllers;
using Orm.Application.Dtos;
using Orm.Application.Orders.Commands.CreateOrder;
using Orm.Application.Orders.Commands.DeleteOrder;
using Orm.Application.Orders.Commands.UpdateOrder;
using Orm.Application.Orders.Queries.GetOrderById;

namespace Orm.Api.Tests;

public class OrderControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _controller = new OrderController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetOrderAsync_Returns200WithDto_WhenFound()
    {
        // Arrange
        var dto = new OrderDto { OrderID = 1, CustomerName = "Alice" };
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.Id == 1), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await _controller.GetOrderAsync(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Equal(dto, okResult.Value);
    }

    [Fact]
    public async Task GetOrderAsync_Returns404_WhenNotFound()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.Is<GetOrderByIdQuery>(q => q.Id == 999), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.GetOrderAsync(999);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateOrderAsync_Returns201Created_WithLocationHeader()
    {
        // Arrange
        var createDto = new CreateOrderDto { CustomerName = "Bob" };
        var resultDto = new OrderDto { OrderID = 42, CustomerName = "Bob" };
        _mediatorMock
            .Setup(m => m.Send(It.Is<CreateOrderCommand>(c => c.CreateOrderDto == createDto), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        // Act
        var result = await _controller.CreateOrderAsync(createDto);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(201, createdResult.StatusCode);
        Assert.Equal(resultDto, createdResult.Value);
        Assert.Equal("GetOrder", createdResult.ActionName);
        Assert.Equal(42L, createdResult.RouteValues!["id"]);
    }

    [Fact]
    public async Task UpdateOrderAsync_Returns400_WhenIdMismatch()
    {
        // Arrange
        var updateDto = new UpdateOrderDto { OrderID = 5 };

        // Act
        var result = await _controller.UpdateOrderAsync(updateDto, 10);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateOrderAsync_Returns400_WhenIdIsZeroOrNegative()
    {
        // Arrange
        var updateDto = new UpdateOrderDto { OrderID = 0 };

        // Act
        var result = await _controller.UpdateOrderAsync(updateDto, 0);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateOrderAsync_Returns200_WithUpdatedDto()
    {
        // Arrange
        var updateDto = new UpdateOrderDto { OrderID = 3, CustomerName = "Carol" };
        var resultDto = new OrderDto { OrderID = 3, CustomerName = "Carol" };
        _mediatorMock
            .Setup(m => m.Send(It.Is<UpdateOrderCommand>(c => c.UpdateOrderDto == updateDto), It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultDto);

        // Act
        var result = await _controller.UpdateOrderAsync(updateDto, 3);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(resultDto, okResult.Value);
    }

    [Fact]
    public async Task DeleteOrderAsync_Returns200_OnSuccess()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.Is<DeleteOrderCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteOrderAsync(1);

        // Assert
        Assert.IsType<OkResult>(result);
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteOrderCommand>(c => c.Id == 1), It.IsAny<CancellationToken>()), Times.Once);
    }
}

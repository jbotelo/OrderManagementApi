using MediatR;
using Moq;
using Orm.Application.Dtos;
using Orm.Application.Orders.Commands.CreateOrder;
using Orm.Application.Orders.Commands.DeleteOrder;
using Orm.Application.Orders.Commands.UpdateOrder;
using Orm.Application.Orders.Notifications;
using Orm.Application.Orders.Queries.GetOrderById;
using Orm.Application.Services;
using Orm.Domain.Entities;
using Orm.Domain.Interfaces;

namespace Orm.Application.Tests;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _handler = new CreateOrderCommandHandler(_repoMock.Object, _mapperMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task Handle_BuildsEntityAndReturnsMappedDto()
    {
        // Arrange
        var createDto = new CreateOrderDto
        {
            CustomerName = "Alice",
            OrderItems =
            [
                new CreateOrderItemDto { ProductId = 1, Quantity = 2, Price = 10m }
            ]
        };

        var expectedDto = new OrderDto { OrderID = 1, CustomerName = "Alice" };

        _repoMock
            .Setup(r => r.CreateAsync(It.Is<Order>(o =>
                o.CustomerName == "Alice" &&
                o.OrderItems.Count == 1 &&
                o.OrderItems[0].ProductId == 1 &&
                o.OrderItems[0].Quantity == 2 &&
                o.OrderItems[0].Price == 10m)))
            .ReturnsAsync((Order o) => o);

        _mapperMock
            .Setup(m => m.MapToDto(It.IsAny<Order>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(new CreateOrderCommand(createDto), CancellationToken.None);

        // Assert
        Assert.Equal(expectedDto, result);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Once);
        _publisherMock.Verify(p => p.Publish(
            It.Is<OrderPersistedNotification>(n => n.Order.CustomerName == "Alice"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _handler = new GetOrderByIdQueryHandler(_repoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenRepoReturnsNull()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(new GetOrderByIdQuery(999), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_ReturnsMappedDto_WhenOrderExists()
    {
        // Arrange
        var order = new Order { OrderID = 1, CustomerName = "Bob", OrderItems = [] };
        var dto = new OrderDto { OrderID = 1, CustomerName = "Bob" };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        _mapperMock.Setup(m => m.MapToDto(order)).Returns(dto);

        // Act
        var result = await _handler.Handle(new GetOrderByIdQuery(1), CancellationToken.None);

        // Assert
        Assert.Equal(dto, result);
    }
}

public class UpdateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IPublisher> _publisherMock = new();
    private readonly UpdateOrderCommandHandler _handler;

    public UpdateOrderCommandHandlerTests()
    {
        _handler = new UpdateOrderCommandHandler(_repoMock.Object, _mapperMock.Object, _publisherMock.Object);
    }

    [Fact]
    public async Task Handle_BuildsEntityAndReturnsMappedDto()
    {
        // Arrange
        var updateDto = new UpdateOrderDto
        {
            OrderID = 5,
            CustomerName = "Carol",
            OrderItems =
            [
                new UpdateOrderItemDto { ProductId = 10, Quantity = 3, Price = 15m }
            ]
        };

        var expectedDto = new OrderDto { OrderID = 5, CustomerName = "Carol" };

        _repoMock
            .Setup(r => r.UpdateAsync(It.Is<Order>(o =>
                o.OrderID == 5 &&
                o.OrderItems.Count == 1 &&
                o.OrderItems[0].ProductId == 10)))
            .ReturnsAsync((Order o) => o);

        _mapperMock
            .Setup(m => m.MapToDto(It.IsAny<Order>()))
            .Returns(expectedDto);

        // Act
        var result = await _handler.Handle(new UpdateOrderCommand(updateDto), CancellationToken.None);

        // Assert
        Assert.Equal(expectedDto, result);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
        _publisherMock.Verify(p => p.Publish(
            It.Is<OrderPersistedNotification>(n => n.Order.OrderID == 5),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class DeleteOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _repoMock = new();
    private readonly DeleteOrderCommandHandler _handler;

    public DeleteOrderCommandHandlerTests()
    {
        _handler = new DeleteOrderCommandHandler(_repoMock.Object);
    }

    [Fact]
    public async Task Handle_DelegatesToRepository()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteAsync(42)).Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(new DeleteOrderCommand(42), CancellationToken.None);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(42), Times.Once);
    }
}

using Moq;
using Orm.Application.Dtos;
using Orm.Application.Services;
using Orm.Domain.Entities;
using Orm.Domain.Interfaces;

namespace Orm.Application.Tests;

public class OrderServiceTests
{
    private readonly Mock<IOrderRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _service = new OrderService(_repoMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_BuildsEntityAndReturnsMappedDto()
    {
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

        var result = await _service.CreateOrderAsync(createDto);

        Assert.Equal(expectedDto, result);
        _repoMock.Verify(r => r.CreateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsNull_WhenRepoReturnsNull()
    {
        _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Order?)null);

        var result = await _service.GetOrderByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ReturnsMappedDto_WhenOrderExists()
    {
        var order = new Order { OrderID = 1, CustomerName = "Bob", OrderItems = [] };
        var dto = new OrderDto { OrderID = 1, CustomerName = "Bob" };

        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(order);
        _mapperMock.Setup(m => m.MapToDto(order)).Returns(dto);

        var result = await _service.GetOrderByIdAsync(1);

        Assert.Equal(dto, result);
    }

    [Fact]
    public async Task UpdateOrderAsync_BuildsEntityAndReturnsMappedDto()
    {
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

        var result = await _service.UpdateOrderAsync(updateDto);

        Assert.Equal(expectedDto, result);
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Order>()), Times.Once);
    }

    [Fact]
    public async Task DeleteOrderAsync_DelegatesToRepository()
    {
        _repoMock.Setup(r => r.DeleteAsync(42)).Returns(Task.CompletedTask);

        await _service.DeleteOrderAsync(42);

        _repoMock.Verify(r => r.DeleteAsync(42), Times.Once);
    }
}

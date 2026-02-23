using Microsoft.Extensions.Logging;
using Moq;
using Orm.Application.Orders.Notifications;
using Orm.Domain.Entities;

namespace Orm.Application.Tests.Orders.Notifications;

public class OrderPersistedCosmosHandlerTests
{
    private readonly Mock<IOrderDocumentStore> _documentStoreMock = new();
    private readonly Mock<ILogger<OrderPersistedCosmosHandler>> _loggerMock = new();
    private readonly OrderPersistedCosmosHandler _handler;

    public OrderPersistedCosmosHandlerTests()
    {
        _handler = new OrderPersistedCosmosHandler(_documentStoreMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_CallsUpsertOrderAsync_WithCorrectOrder()
    {
        // Arrange
        var order = new Order
        {
            OrderID = 42,
            CustomerName = "Alice",
            CreateDate = DateTime.UtcNow,
            OrderItems = [new OrderItem { OrderItemID = 1, ProductId = 10, Quantity = 2, Price = 5m }]
        };
        var notification = new OrderPersistedNotification(order);

        // Act
        await _handler.Handle(notification, CancellationToken.None);

        // Assert
        _documentStoreMock.Verify(
            s => s.UpsertOrderAsync(order, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_SwallowsException_WhenDocumentStoreFails()
    {
        // Arrange
        var order = new Order
        {
            OrderID = 99,
            CustomerName = "Bob",
            OrderItems = []
        };
        var notification = new OrderPersistedNotification(order);

        _documentStoreMock
            .Setup(s => s.UpsertOrderAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("CosmosDB unavailable"));

        // Act & Assert — should not throw
        var exception = await Record.ExceptionAsync(
            () => _handler.Handle(notification, CancellationToken.None));

        Assert.Null(exception);
    }
}

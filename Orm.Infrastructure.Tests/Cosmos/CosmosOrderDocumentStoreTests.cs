using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Orm.Domain.Entities;
using Orm.Infrastructure.Cosmos;

namespace Orm.Infrastructure.Tests.Cosmos;

public class CosmosOrderDocumentStoreTests
{
    private readonly Mock<Container> _containerMock = new();
    private readonly Mock<ILogger<CosmosOrderDocumentStore>> _loggerMock = new();
    private readonly CosmosOrderDocumentStore _store;

    public CosmosOrderDocumentStoreTests()
    {
        var settings = Options.Create(new CosmosDbSettings
        {
            DatabaseName = "TestDb",
            ContainerName = "TestContainer"
        });

        var cosmosClientMock = new Mock<CosmosClient>();
        cosmosClientMock
            .Setup(c => c.GetContainer("TestDb", "TestContainer"))
            .Returns(_containerMock.Object);

        _store = new CosmosOrderDocumentStore(cosmosClientMock.Object, settings, _loggerMock.Object);
    }

    [Fact]
    public async Task UpsertOrderAsync_CallsUpsertItemAsync_WithCorrectDocument()
    {
        // Arrange
        var order = new Order
        {
            OrderID = 42,
            CustomerName = "Alice",
            CreateDate = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc),
            OrderItems =
            [
                new OrderItem { OrderItemID = 101, ProductId = 1, Quantity = 2, Price = 10.50m },
                new OrderItem { OrderItemID = 102, ProductId = 2, Quantity = 1, Price = 5.00m }
            ]
        };

        _containerMock
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<OrderDocument>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<OrderDocument>>());

        // Act
        await _store.UpsertOrderAsync(order);

        // Assert
        _containerMock.Verify(c => c.UpsertItemAsync(
            It.Is<OrderDocument>(d =>
                d.Id == "42" &&
                d.OrderId == 42 &&
                d.CustomerName == "Alice" &&
                d.ItemCount == 2 &&
                d.TotalAmount == 26.00m &&
                d.Items.Count == 2),
            It.Is<PartitionKey>(pk => pk.Equals(new PartitionKey("42"))),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertOrderAsync_ComputesTotalAmount_Correctly()
    {
        // Arrange
        var order = new Order
        {
            OrderID = 1,
            CustomerName = "Bob",
            CreateDate = DateTime.UtcNow,
            OrderItems =
            [
                new OrderItem { OrderItemID = 1, ProductId = 1, Quantity = 3, Price = 10.00m },
                new OrderItem { OrderItemID = 2, ProductId = 2, Quantity = 2, Price = 7.50m }
            ]
        };

        _containerMock
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<OrderDocument>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<OrderDocument>>());

        // Act
        await _store.UpsertOrderAsync(order);

        // Assert — total = (3 * 10) + (2 * 7.5) = 45
        _containerMock.Verify(c => c.UpsertItemAsync(
            It.Is<OrderDocument>(d => d.TotalAmount == 45.00m),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpsertOrderAsync_HandlesEmptyOrderItems()
    {
        // Arrange
        var order = new Order
        {
            OrderID = 10,
            CustomerName = "Carol",
            CreateDate = DateTime.UtcNow,
            OrderItems = []
        };

        _containerMock
            .Setup(c => c.UpsertItemAsync(
                It.IsAny<OrderDocument>(),
                It.IsAny<PartitionKey>(),
                It.IsAny<ItemRequestOptions>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<ItemResponse<OrderDocument>>());

        // Act
        await _store.UpsertOrderAsync(order);

        // Assert
        _containerMock.Verify(c => c.UpsertItemAsync(
            It.Is<OrderDocument>(d =>
                d.Id == "10" &&
                d.ItemCount == 0 &&
                d.TotalAmount == 0m &&
                d.Items.Count == 0),
            It.IsAny<PartitionKey>(),
            It.IsAny<ItemRequestOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}

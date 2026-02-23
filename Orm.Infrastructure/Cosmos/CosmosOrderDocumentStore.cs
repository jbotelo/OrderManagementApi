using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orm.Application.Orders.Notifications;
using Orm.Domain.Entities;

namespace Orm.Infrastructure.Cosmos;

public class CosmosOrderDocumentStore : IOrderDocumentStore
{
    private readonly Container _container;
    private readonly ILogger<CosmosOrderDocumentStore> _logger;

    public CosmosOrderDocumentStore(
        CosmosClient cosmosClient,
        IOptions<CosmosDbSettings> settings,
        ILogger<CosmosOrderDocumentStore> logger)
    {
        _logger = logger;
        var config = settings.Value;
        _container = cosmosClient.GetContainer(config.DatabaseName, config.ContainerName);
    }

    public async Task UpsertOrderAsync(Order order, CancellationToken cancellationToken = default)
    {
        var document = MapToDocument(order);
        var partitionKey = new PartitionKey(document.Id);

        await _container.UpsertItemAsync(document, partitionKey, cancellationToken: cancellationToken);

        _logger.LogDebug("Upserted order document {OrderId} to CosmosDB", order.OrderID);
    }

    private static OrderDocument MapToDocument(Order order)
    {
        var items = order.OrderItems.Select(oi => new OrderItemDocument
        {
            OrderItemId = oi.OrderItemID,
            ProductId = oi.ProductId,
            Quantity = oi.Quantity,
            Price = oi.Price
        }).ToList();

        return new OrderDocument
        {
            Id = order.OrderID.ToString(),
            OrderId = order.OrderID,
            CustomerName = order.CustomerName,
            CreateDate = order.CreateDate,
            LastModifiedDate = DateTime.UtcNow,
            Items = items,
            TotalAmount = items.Sum(i => i.Price * i.Quantity),
            ItemCount = items.Count
        };
    }
}

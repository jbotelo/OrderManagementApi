using MediatR;
using Microsoft.Extensions.Logging;

namespace Orm.Application.Orders.Notifications;

public class OrderPersistedCosmosHandler : INotificationHandler<OrderPersistedNotification>
{
    private readonly IOrderDocumentStore _documentStore;
    private readonly ILogger<OrderPersistedCosmosHandler> _logger;

    public OrderPersistedCosmosHandler(
        IOrderDocumentStore documentStore,
        ILogger<OrderPersistedCosmosHandler> logger)
    {
        _documentStore = documentStore;
        _logger = logger;
    }

    public async Task Handle(OrderPersistedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await _documentStore.UpsertOrderAsync(notification.Order, cancellationToken);
            _logger.LogInformation("Order {OrderId} synced to CosmosDB", notification.Order.OrderID);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to sync order {OrderId} to CosmosDB. SQL Server remains source of truth.",
                notification.Order.OrderID);
        }
    }
}

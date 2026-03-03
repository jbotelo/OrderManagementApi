using Orm.Domain.Entities;

namespace Orm.Application.Orders.Notifications;

public interface IOrderDocumentStore
{
    Task UpsertOrderAsync(Order order, CancellationToken cancellationToken = default);
}

using MediatR;
using Orm.Domain.Entities;

namespace Orm.Application.Orders.Notifications;

public record OrderPersistedNotification(Order Order) : INotification;

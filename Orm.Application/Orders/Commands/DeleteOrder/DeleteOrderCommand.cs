using MediatR;

namespace Orm.Application.Orders.Commands.DeleteOrder;

public record DeleteOrderCommand(long Id) : IRequest;

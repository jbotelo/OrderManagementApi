using MediatR;
using Orm.Application.Dtos;

namespace Orm.Application.Orders.Commands.UpdateOrder;

public record UpdateOrderCommand(UpdateOrderDto UpdateOrderDto) : IRequest<OrderDto>;

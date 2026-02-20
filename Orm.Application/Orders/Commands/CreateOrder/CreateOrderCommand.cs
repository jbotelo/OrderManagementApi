using MediatR;
using Orm.Application.Dtos;

namespace Orm.Application.Orders.Commands.CreateOrder;

public record CreateOrderCommand(CreateOrderDto CreateOrderDto) : IRequest<OrderDto>;

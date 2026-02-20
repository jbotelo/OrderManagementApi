using MediatR;
using Orm.Application.Dtos;

namespace Orm.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(long Id) : IRequest<OrderDto?>;

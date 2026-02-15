using Orm.Application.Dtos;
using Orm.Domain.Entities;

namespace Orm.Application.Services
{
    public interface IMapper
    {
        OrderDto MapToDto(Order order);
        OrderItemDto MapToDto(OrderItem orderItem);
        Order MapToEntity(OrderDto orderDto);
        OrderItem MapToEntity(OrderItemDto orderItemDto);
    }
}
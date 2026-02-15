using Orm.Application.Dtos;
using Orm.Domain.Entities;

namespace Orm.Application.Services
{
    public class Mapper : IMapper
    {
        public OrderItemDto MapToDto(OrderItem orderItem)
        {
            return new OrderItemDto
            {
                OrderItemID = orderItem.OrderItemID,
                ProductId = orderItem.ProductId,
                Quantity = orderItem.Quantity,
                Price = orderItem.Price
            };
        }

        public OrderItem MapToEntity(OrderItemDto orderItemDto)
        {
            return new OrderItem
            {
                OrderItemID = orderItemDto.OrderItemID,
                ProductId = orderItemDto.ProductId,
                Quantity = orderItemDto.Quantity,
                Price = orderItemDto.Price
            };
        }

        public OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                OrderID = order.OrderID,
                CustomerName = order.CustomerName,
                CreateDate = order.CreateDate,
                OrderItems = order.OrderItems != null ? order.OrderItems.ConvertAll(MapToDto) : new List<OrderItemDto>()
            };
        }

        public Order MapToEntity(OrderDto orderDto)
        {
            return new Order
            {
                OrderID = orderDto.OrderID,
                CustomerName = orderDto.CustomerName,
                CreateDate = orderDto.CreateDate,
                OrderItems = orderDto.OrderItems != null ? orderDto.OrderItems.ConvertAll(MapToEntity) : new List<OrderItem>()
            };
        }
    }
}

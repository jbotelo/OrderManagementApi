using Orm.Application.Dtos;

namespace Orm.Application.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrder(CreateOrderDto createOrderDto);
        Task DeleteOrder(long id);
        Task<OrderDto> GetOrderById(long id);
        Task<OrderDto> UpdateOrder(UpdateOrderDto updateOrderDto);
    }
}
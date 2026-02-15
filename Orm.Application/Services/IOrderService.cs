using Orm.Application.Dtos;

namespace Orm.Application.Services
{
    public interface IOrderService
    {
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task DeleteOrderAsync(long id);
        Task<OrderDto> GetOrderByIdAsync(long id);
        Task<OrderDto> UpdateOrderAsync(UpdateOrderDto updateOrderDto);
    }
}
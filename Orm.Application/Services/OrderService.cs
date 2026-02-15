using Orm.Application.Dtos;
using Orm.Domain.Entities;
using Orm.Domain.Interfaces;

namespace Orm.Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IMapper mapper)
        {
            this._orderRepository = orderRepository;
            this._mapper = mapper;
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var order = new Order
            {
                CreateDate = DateTime.UtcNow,
                CustomerName = createOrderDto.CustomerName,
                OrderItems = createOrderDto.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };

            var o = await _orderRepository.CreateAsync(order);

            return _mapper.MapToDto(o);
        }

        public async Task<OrderDto> UpdateOrderAsync(UpdateOrderDto updateOrderDto)
        {
            var order = new Order
            {
                OrderID = updateOrderDto.OrderID,
                OrderItems = updateOrderDto.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList(),
            };
            var o = await _orderRepository.UpdateAsync(order);

            return _mapper.MapToDto(o);
        }

        public async Task<OrderDto> GetOrderByIdAsync(long id)
        {
            var o = await _orderRepository.GetByIdAsync(id);
            return _mapper.MapToDto(o);
        }

        public async Task DeleteOrderAsync(long id)
        {
            await _orderRepository.DeleteAsync(id);
        }
    }
}

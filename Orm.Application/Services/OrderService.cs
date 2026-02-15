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

        public async Task<OrderDto> CreateOrder(CreateOrderDto createOrderDto)
        {
            var order = new Order
            {
                CreateDate = createOrderDto.CreateDate,
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

        public async Task<OrderDto> UpdateOrder(UpdateOrderDto updateOrderDto)
        {
            var order = new Order
            {
                OrderID = updateOrderDto.OrderID,
                CreateDate = updateOrderDto.CreateDate,
                OrderItems = updateOrderDto.OrderItems.Select(oi => new OrderItem
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };
            var o = await _orderRepository.UpdateAsync(order);

            return _mapper.MapToDto(o);
        }

        public async Task<OrderDto> GetOrderById(long id)
        {
            var o = await _orderRepository.GetByIdAsync(id);
            return _mapper.MapToDto(o);
        }

        public async Task DeleteOrder(long id)
        {
            await _orderRepository.DeleteAsync(id);
        }
    }
}

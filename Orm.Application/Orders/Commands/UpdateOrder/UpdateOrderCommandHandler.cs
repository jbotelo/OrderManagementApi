using MediatR;
using Orm.Application.Dtos;
using Orm.Application.Services;
using Orm.Domain.Entities;
using Orm.Domain.Interfaces;

namespace Orm.Application.Orders.Commands.UpdateOrder;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;

    public UpdateOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
    }

    public async Task<OrderDto> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            OrderID = request.UpdateOrderDto.OrderID,
            OrderItems = request.UpdateOrderDto.OrderItems.Select(oi => new OrderItem
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList(),
        };

        var updatedOrder = await _orderRepository.UpdateAsync(order);
        return _mapper.MapToDto(updatedOrder);
    }
}

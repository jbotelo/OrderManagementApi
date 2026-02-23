using MediatR;
using Orm.Application.Dtos;
using Orm.Application.Orders.Notifications;
using Orm.Application.Services;
using Orm.Domain.Entities;
using Orm.Domain.Interfaces;

namespace Orm.Application.Orders.Commands.UpdateOrder;

public class UpdateOrderCommandHandler : IRequestHandler<UpdateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    public UpdateOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IPublisher publisher)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _publisher = publisher;
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
        await _publisher.Publish(new OrderPersistedNotification(updatedOrder), cancellationToken);
        return _mapper.MapToDto(updatedOrder);
    }
}

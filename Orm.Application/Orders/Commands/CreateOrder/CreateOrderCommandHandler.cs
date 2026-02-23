using MediatR;
using Orm.Application.Dtos;
using Orm.Application.Orders.Notifications;
using Orm.Application.Services;
using Orm.Domain.Entities;
using Orm.Domain.Interfaces;

namespace Orm.Application.Orders.Commands.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderDto>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IMapper _mapper;
    private readonly IPublisher _publisher;

    public CreateOrderCommandHandler(IOrderRepository orderRepository, IMapper mapper, IPublisher publisher)
    {
        _orderRepository = orderRepository;
        _mapper = mapper;
        _publisher = publisher;
    }

    public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            CreateDate = DateTime.UtcNow,
            CustomerName = request.CreateOrderDto.CustomerName,
            OrderItems = request.CreateOrderDto.OrderItems.Select(oi => new OrderItem
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };

        var createdOrder = await _orderRepository.CreateAsync(order);
        await _publisher.Publish(new OrderPersistedNotification(createdOrder), cancellationToken);
        return _mapper.MapToDto(createdOrder);
    }
}

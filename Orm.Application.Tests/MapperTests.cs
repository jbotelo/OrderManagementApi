using Orm.Application.Dtos;
using Orm.Application.Services;
using Orm.Domain.Entities;

namespace Orm.Application.Tests;

public class MapperTests
{
    private readonly Mapper _mapper = new();

    [Fact]
    public void MapToDto_Order_MapsAllProperties()
    {
        var order = new Order
        {
            OrderID = 1,
            CreateDate = new DateTime(2025, 1, 15, 10, 30, 0),
            CustomerName = "Alice",
            OrderItems =
            [
                new OrderItem { OrderItemID = 10, OrderID = 1, ProductId = 100, Quantity = 2, Price = 9.99m }
            ]
        };

        var dto = _mapper.MapToDto(order);

        Assert.Equal(order.OrderID, dto.OrderID);
        Assert.Equal(order.CreateDate, dto.CreateDate);
        Assert.Equal(order.CustomerName, dto.CustomerName);
        Assert.Single(dto.OrderItems);
        Assert.Equal(10, dto.OrderItems[0].OrderItemID);
        Assert.Equal(1, dto.OrderItems[0].OrderID);
        Assert.Equal(100, dto.OrderItems[0].ProductId);
        Assert.Equal(2, dto.OrderItems[0].Quantity);
        Assert.Equal(9.99m, dto.OrderItems[0].Price);
    }

    [Fact]
    public void MapToDto_OrderItem_MapsAllProperties()
    {
        var item = new OrderItem { OrderItemID = 5, OrderID = 3, ProductId = 42, Quantity = 7, Price = 19.50m };

        var dto = _mapper.MapToDto(item);

        Assert.Equal(5, dto.OrderItemID);
        Assert.Equal(3, dto.OrderID);
        Assert.Equal(42, dto.ProductId);
        Assert.Equal(7, dto.Quantity);
        Assert.Equal(19.50m, dto.Price);
    }

    [Fact]
    public void MapToEntity_OrderDto_MapsAllProperties()
    {
        var dto = new OrderDto
        {
            OrderID = 2,
            CreateDate = new DateTime(2025, 6, 1),
            CustomerName = "Bob",
            OrderItems =
            [
                new OrderItemDto { OrderItemID = 20, OrderID = 2, ProductId = 200, Quantity = 1, Price = 5.00m }
            ]
        };

        var entity = _mapper.MapToEntity(dto);

        Assert.Equal(dto.OrderID, entity.OrderID);
        Assert.Equal(dto.CreateDate, entity.CreateDate);
        Assert.Equal(dto.CustomerName, entity.CustomerName);
        Assert.Single(entity.OrderItems);
        Assert.Equal(20, entity.OrderItems[0].OrderItemID);
        Assert.Equal(200, entity.OrderItems[0].ProductId);
        Assert.Equal(1, entity.OrderItems[0].Quantity);
        Assert.Equal(5.00m, entity.OrderItems[0].Price);
    }

    [Fact]
    public void MapToEntity_OrderItemDto_MapsAllProperties()
    {
        var dto = new OrderItemDto { OrderItemID = 8, OrderID = 4, ProductId = 99, Quantity = 3, Price = 12.75m };

        var entity = _mapper.MapToEntity(dto);

        Assert.Equal(8, entity.OrderItemID);
        Assert.Equal(99, entity.ProductId);
        Assert.Equal(3, entity.Quantity);
        Assert.Equal(12.75m, entity.Price);
    }

    [Fact]
    public void MapToDto_Order_WithEmptyItems_ReturnsEmptyList()
    {
        var order = new Order { OrderID = 1, CustomerName = "Test", OrderItems = [] };

        var dto = _mapper.MapToDto(order);

        Assert.Empty(dto.OrderItems);
    }
}

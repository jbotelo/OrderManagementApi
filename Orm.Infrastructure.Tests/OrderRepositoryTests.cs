using Microsoft.EntityFrameworkCore;
using Orm.Domain.Entities;
using Orm.Infrastructure.DbContexts;
using Orm.Infrastructure.Repositories;

namespace Orm.Infrastructure.Tests;

public class OrderRepositoryTests
{
    private static AppDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_PersistsOrderWithItems()
    {
        // Arrange
        using var context = CreateContext(nameof(CreateAsync_PersistsOrderWithItems));
        var repo = new OrderRepository(context);

        var order = new Order
        {
            CustomerName = "Alice",
            CreateDate = DateTime.UtcNow,
            OrderItems =
            [
                new OrderItem { ProductId = 1, Quantity = 2, Price = 10m },
                new OrderItem { ProductId = 2, Quantity = 1, Price = 5m }
            ]
        };

        // Act
        var result = await repo.CreateAsync(order);

        // Assert
        Assert.True(result.OrderID > 0);
        Assert.Equal(2, result.OrderItems.Count);

        var persisted = await context.Orders.Include(o => o.OrderItems).SingleAsync();
        Assert.Equal("Alice", persisted.CustomerName);
        Assert.Equal(2, persisted.OrderItems.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsOrderWithItems()
    {
        // Arrange
        using var context = CreateContext(nameof(GetByIdAsync_ReturnsOrderWithItems));
        var order = new Order
        {
            CustomerName = "Bob",
            CreateDate = DateTime.UtcNow,
            OrderItems = [new OrderItem { ProductId = 5, Quantity = 3, Price = 7m }]
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var repo = new OrderRepository(context);

        // Act
        var result = await repo.GetByIdAsync(order.OrderID);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Bob", result.CustomerName);
        Assert.Single(result.OrderItems);
        Assert.Equal(5, result.OrderItems[0].ProductId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_ForMissingId()
    {
        // Arrange
        using var context = CreateContext(nameof(GetByIdAsync_ReturnsNull_ForMissingId));
        var repo = new OrderRepository(context);

        // Act
        var result = await repo.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesExistingItems()
    {
        // Arrange
        using var context = CreateContext(nameof(UpdateAsync_ModifiesExistingItems));
        var order = new Order
        {
            CustomerName = "Carol",
            CreateDate = DateTime.UtcNow,
            OrderItems = [new OrderItem { ProductId = 10, Quantity = 1, Price = 20m }]
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repo = new OrderRepository(context);
        var updateOrder = new Order
        {
            OrderID = order.OrderID,
            OrderItems = [new OrderItem { ProductId = 10, Quantity = 5, Price = 25m }]
        };

        // Act
        var result = await repo.UpdateAsync(updateOrder);

        // Assert
        context.ChangeTracker.Clear();
        var persisted = await context.Orders.Include(o => o.OrderItems).SingleAsync();
        Assert.Single(persisted.OrderItems);
        Assert.Equal(5, persisted.OrderItems[0].Quantity);
        Assert.Equal(25m, persisted.OrderItems[0].Price);
    }

    [Fact]
    public async Task UpdateAsync_DeletesRemovedItems_AddsNewItems()
    {
        // Arrange
        using var context = CreateContext(nameof(UpdateAsync_DeletesRemovedItems_AddsNewItems));
        var order = new Order
        {
            CustomerName = "Dave",
            CreateDate = DateTime.UtcNow,
            OrderItems =
            [
                new OrderItem { ProductId = 1, Quantity = 1, Price = 10m },
                new OrderItem { ProductId = 2, Quantity = 1, Price = 20m }
            ]
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repo = new OrderRepository(context);
        // Remove ProductId=1, keep ProductId=2, add ProductId=3
        var updateOrder = new Order
        {
            OrderID = order.OrderID,
            OrderItems =
            [
                new OrderItem { ProductId = 2, Quantity = 2, Price = 22m },
                new OrderItem { ProductId = 3, Quantity = 4, Price = 30m }
            ]
        };

        // Act
        await repo.UpdateAsync(updateOrder);

        // Assert
        context.ChangeTracker.Clear();
        var persisted = await context.Orders.Include(o => o.OrderItems).SingleAsync();
        Assert.Equal(2, persisted.OrderItems.Count);
        Assert.DoesNotContain(persisted.OrderItems, i => i.ProductId == 1);
        Assert.Contains(persisted.OrderItems, i => i.ProductId == 2 && i.Quantity == 2);
        Assert.Contains(persisted.OrderItems, i => i.ProductId == 3 && i.Quantity == 4);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsInvalidOperationException_ForNonExistentOrder()
    {
        // Arrange
        using var context = CreateContext(nameof(UpdateAsync_ThrowsInvalidOperationException_ForNonExistentOrder));
        var repo = new OrderRepository(context);
        var order = new Order { OrderID = 999, OrderItems = [] };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.UpdateAsync(order));
    }

    [Fact]
    public async Task DeleteAsync_RemovesOrderAndItems()
    {
        // Arrange
        using var context = CreateContext(nameof(DeleteAsync_RemovesOrderAndItems));
        var order = new Order
        {
            CustomerName = "Eve",
            CreateDate = DateTime.UtcNow,
            OrderItems = [new OrderItem { ProductId = 7, Quantity = 1, Price = 15m }]
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repo = new OrderRepository(context);

        // Act
        await repo.DeleteAsync(order.OrderID);

        // Assert
        Assert.Empty(await context.Orders.ToListAsync());
        Assert.Empty(await context.OrderItems.ToListAsync());
    }

    [Fact]
    public async Task DeleteAsync_ThrowsInvalidOperationException_ForNonExistentOrder()
    {
        // Arrange
        using var context = CreateContext(nameof(DeleteAsync_ThrowsInvalidOperationException_ForNonExistentOrder));
        var repo = new OrderRepository(context);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.DeleteAsync(999));
    }
}

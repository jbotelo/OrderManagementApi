using Microsoft.EntityFrameworkCore;
using Orm.Domain.Entities;
using Orm.Domain.Interfaces;
using Orm.Infrastructure.DbContexts;

namespace Orm.Infrastructure.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context)
        {
            this._context = context;
        }

        public async Task<Order> CreateAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task DeleteAsync(long id)
        {
            var existentOrder = await _context.Orders.AsNoTracking().Include(o => o.OrderItems).SingleOrDefaultAsync(o => o.OrderID == id);
            if (existentOrder == null)
            {
                throw new InvalidOperationException("Order not found for deletion");
            }

            existentOrder.OrderItems.ForEach(item => _context.Entry(item).State = EntityState.Deleted);

            _context.Entry(existentOrder).State = EntityState.Deleted;
            await _context.SaveChangesAsync();
        }

        public async Task<Order?> GetByIdAsync(long id)
        {
            var getOrder = await _context.Orders.AsNoTracking().Include(o => o.OrderItems).SingleOrDefaultAsync(o => o.OrderID == id);
            return getOrder;
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            var existentOrder = await _context.Orders.AsNoTracking().Include(o => o.OrderItems).SingleOrDefaultAsync(o => o.OrderID == order.OrderID);
            if (existentOrder == null)
            {
                throw new InvalidOperationException("Order not found for update");
            }

            existentOrder.OrderItems.ForEach(existentItem =>
            {
                var incomeItem = order.OrderItems.SingleOrDefault(i => i.ProductId == existentItem.ProductId);
                if (incomeItem != null)
                {
                    existentItem.Quantity = incomeItem.Quantity;
                    existentItem.Price = incomeItem.Price;
                    _context.Entry(existentItem).State = EntityState.Modified;
                }
                else
                { 
                    _context.Entry(existentItem).State = EntityState.Deleted;
                }
            });

            foreach (var newInboundItem in order.OrderItems.Where(oi => existentOrder.OrderItems.All(ei => ei.ProductId != oi.ProductId)))
            {
                var newItem = new OrderItem
                {
                    ProductId = newInboundItem.ProductId,
                    Quantity = newInboundItem.Quantity,
                    Price = newInboundItem.Price,
                    OrderID = order.OrderID
                };
                await _context.OrderItems.AddAsync(newItem);
            }

            _context.Entry(existentOrder).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return existentOrder;
        }
    }
}
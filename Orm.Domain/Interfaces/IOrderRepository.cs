using Orm.Domain.Entities;

namespace Orm.Domain.Interfaces
{
    public interface IOrderRepository
    {
        public Task<Order?> GetByIdAsync(long id);
        public Task<Order> CreateAsync(Order order);
        public Task<Order> UpdateAsync(Order order);
        public Task DeleteAsync(long id);
    }
}
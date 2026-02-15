using Orm.Domain.Entities;

namespace Orm.Domain.Interfaces
{
    public interface IOrderItemRepository
    {
        public Task<OrderItem> GetByIdAsync(int id);
        public Task<OrderItem> CreateAsync(OrderItem orderItem);
        public Task<OrderItem> UpdateAsync(OrderItem orderItem);
        public Task DeleteAsync(int id);
    }
}

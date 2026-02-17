namespace Orm.Domain.Entities
{
    public class Order
    {
        public long OrderID { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CustomerName { get; set; }
        public List<OrderItem> OrderItems { get; set; } = [];
    }
}

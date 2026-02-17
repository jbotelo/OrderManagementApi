namespace Orm.Application.Dtos
{
    public class OrderDto
    {
        public long OrderID { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CustomerName { get; set; }
        public List<OrderItemDto> OrderItems { get; set; } = [];
    }
}

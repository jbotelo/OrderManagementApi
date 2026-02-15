namespace Orm.Application.Dtos
{
    public class OrderItemDto
    {
        public long OrderItemID { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? CustomerName { get; set; }
    }
}

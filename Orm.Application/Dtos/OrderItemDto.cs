namespace Orm.Application.Dtos
{
    public class OrderItemDto
    {
        public long OrderItemID { get; set; }
        public long OrderID { get; set; }
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}

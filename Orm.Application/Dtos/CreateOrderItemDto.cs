namespace Orm.Application.Dtos
{
    public class CreateOrderItemDto
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? CustomerName { get; set; }
    }
}

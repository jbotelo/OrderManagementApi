namespace Orm.Application.Dtos
{
    public class UpdateOrderItemDto
    {
        public long ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}

namespace Orm.Application.Dtos
{
    public class CreateOrderDto
    {
        public DateTime CreateDate { get; set; }
        public string? CustomerName { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}

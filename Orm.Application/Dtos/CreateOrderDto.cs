namespace Orm.Application.Dtos
{
    public class CreateOrderDto
    {
        public string? CustomerName { get; set; }
        public List<CreateOrderItemDto> OrderItems { get; set; } = [];
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Orm.Application.Dtos
{
    public class UpdateOrderDto
    {
        public long OrderID { get; set; }
        public DateTime CreateDate { get; set; }
        public string? CustomerName { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}

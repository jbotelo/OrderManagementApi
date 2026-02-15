using System;
using System.Collections.Generic;
using System.Text;

namespace Orm.Application.Dtos
{
    public class UpdateOrderDto
    {
        public long OrderID { get; set; }
        public string? CustomerName { get; set; }
        public List<UpdateOrderItemDto> OrderItems { get; set; }
    }
}

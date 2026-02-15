using Microsoft.AspNetCore.Mvc;
using Orm.Application.Services;

namespace Orm.Api.Controllers
{
    [ApiController]
    [Route("api/v1/order")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            this._orderService = orderService;
        }

        [HttpGet(Name = "GetOrder")]
        [Route("get-order/{id:long}")]
        public async Task<IActionResult> GetOrderAsync(long id)
        {
            var orderDto = await _orderService.GetOrderById(id);
            return Ok(orderDto);
        }
    }
}

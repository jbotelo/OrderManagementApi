using Microsoft.AspNetCore.Mvc;
using Orm.Application.Dtos;
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
            var orderDto = await _orderService.GetOrderByIdAsync(id);
            return Ok(orderDto);
        }

        [HttpPost(Name = "CreateOrder")]
        [Route("create-order")]
        public async Task<IActionResult> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var orderDto = await _orderService.CreateOrderAsync(createOrderDto);
            return CreatedAtAction("GetOrder", new { id = orderDto.OrderID }, orderDto);
        }

        [HttpPut(Name = "UpdateOrder")]
        [Route("update-order/{id:long}")]
        public async Task<IActionResult> UpdateOrderAsync(UpdateOrderDto updateOrderDto, long id)
        {
            if (updateOrderDto?.OrderID != id || id <= 0)
            {
                return BadRequest();
            }
            var orderDto = await _orderService.UpdateOrderAsync(updateOrderDto);
            return Ok(orderDto);
        }

        [HttpDelete(Name = "DeleteOrder")]
        [Route("delete-order/{id:long}")]
        public async Task<IActionResult> DeleteOrderAsync(long id)
        {
            await _orderService.DeleteOrderAsync(id);
            return Ok();
        }

    }
}

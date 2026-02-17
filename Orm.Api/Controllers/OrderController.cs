using Microsoft.AspNetCore.Mvc;
using Orm.Application.Dtos;
using Orm.Application.Services;

namespace Orm.Api.Controllers
{
    [ApiController]
    [Route("api/v1/order")]
    [Produces("application/json")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            this._orderService = orderService;
        }

        [HttpGet("get-order/{id:long}", Name = "GetOrder")]
        [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderAsync(long id)
        {
            var orderDto = await _orderService.GetOrderByIdAsync(id);
            if (orderDto == null) return NotFound();
            return Ok(orderDto);
        }

        [HttpPost("create-order", Name = "CreateOrder")]
        [ProducesResponseType<OrderDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var orderDto = await _orderService.CreateOrderAsync(createOrderDto);
            return CreatedAtAction("GetOrder", new { id = orderDto.OrderID }, orderDto);
        }

        [HttpPut("update-order/{id:long}", Name = "UpdateOrder")]
        [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateOrderAsync(UpdateOrderDto updateOrderDto, long id)
        {
            if (updateOrderDto?.OrderID != id || id <= 0)
            {
                return BadRequest();
            }
            var orderDto = await _orderService.UpdateOrderAsync(updateOrderDto);
            return Ok(orderDto);
        }

        [HttpDelete("delete-order/{id:long}", Name = "DeleteOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrderAsync(long id)
        {
            await _orderService.DeleteOrderAsync(id);
            return Ok();
        }

    }
}

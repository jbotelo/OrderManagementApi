using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orm.Application.Auth;
using Orm.Application.Dtos;
using Orm.Application.Orders.Commands.CreateOrder;
using Orm.Application.Orders.Commands.DeleteOrder;
using Orm.Application.Orders.Commands.UpdateOrder;
using Orm.Application.Orders.Queries.GetOrderById;

namespace Orm.Api.Controllers
{
    [ApiController]
    [Route("api/v1/order")]
    [Produces("application/json")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        [HttpGet("get-order/{id:long}", Name = "GetOrder")]
        [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = AuthorizationPolicies.OrdersRead)]
        public async Task<IActionResult> GetOrderAsync(long id)
        {
            var orderDto = await _mediator.Send(new GetOrderByIdQuery(id));
            if (orderDto == null) return NotFound();
            return Ok(orderDto);
        }

        [HttpPost("create-order", Name = "CreateOrder")]
        [ProducesResponseType<OrderDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthorizationPolicies.OrdersCreate)]
        public async Task<IActionResult> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var orderDto = await _mediator.Send(new CreateOrderCommand(createOrderDto));
            return CreatedAtAction("GetOrder", new { id = orderDto.OrderID }, orderDto);
        }

        [HttpPut("update-order/{id:long}", Name = "UpdateOrder")]
        [ProducesResponseType<OrderDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = AuthorizationPolicies.OrdersUpdate)]
        public async Task<IActionResult> UpdateOrderAsync(UpdateOrderDto updateOrderDto, long id)
        {
            if (updateOrderDto?.OrderID != id || id <= 0)
            {
                return BadRequest();
            }
            var orderDto = await _mediator.Send(new UpdateOrderCommand(updateOrderDto));
            return Ok(orderDto);
        }

        [HttpDelete("delete-order/{id:long}", Name = "DeleteOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = AuthorizationPolicies.OrdersDelete)]
        public async Task<IActionResult> DeleteOrderAsync(long id)
        {
            await _mediator.Send(new DeleteOrderCommand(id));
            return Ok();
        }

    }
}

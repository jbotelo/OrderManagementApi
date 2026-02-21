using MediatR;
using Microsoft.AspNetCore.Mvc;
using Orm.Application.Auth.Commands.Login;
using Orm.Application.Auth.Commands.RefreshToken;
using Orm.Application.Auth.Commands.Register;
using Orm.Application.Dtos;

namespace Orm.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("register")]
        [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            try
            {
                var result = await _mediator.Send(new RegisterCommand(registerDto));
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            try
            {
                var result = await _mediator.Send(new LoginCommand(loginDto));
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return Unauthorized(new { error = "Invalid email or password." });
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType<AuthResponseDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var result = await _mediator.Send(new RefreshTokenCommand(refreshTokenDto));
                return Ok(result);
            }
            catch (InvalidOperationException)
            {
                return Unauthorized(new { error = "Invalid or expired refresh token." });
            }
        }
    }
}

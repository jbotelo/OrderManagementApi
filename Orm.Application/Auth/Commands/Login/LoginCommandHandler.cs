using MediatR;
using Microsoft.AspNetCore.Identity;
using Orm.Application.Dtos;
using Orm.Application.Services;
using Orm.Domain.Entities;

namespace Orm.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var dto = request.LoginDto;

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw new InvalidOperationException("Invalid email or password.");

        var isValidPassword = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isValidPassword)
            throw new InvalidOperationException("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        return await _tokenService.GenerateTokensAsync(user.Id, user.Email!, roles);
    }
}

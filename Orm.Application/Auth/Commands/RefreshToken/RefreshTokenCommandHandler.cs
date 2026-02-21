using MediatR;
using Microsoft.AspNetCore.Identity;
using Orm.Application.Dtos;
using Orm.Application.Services;
using Orm.Domain.Entities;
using Orm.Domain.Interfaces;

namespace Orm.Application.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;

    public RefreshTokenCommandHandler(
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var dto = request.RefreshTokenDto;

        var existingToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken);
        if (existingToken == null || !existingToken.IsActive)
            throw new InvalidOperationException("Invalid or expired refresh token.");

        // Revoke the old refresh token
        existingToken.RevokedAt = DateTime.UtcNow;

        var user = await _userManager.FindByIdAsync(existingToken.UserId);
        if (user == null)
            throw new InvalidOperationException("User not found.");

        var roles = await _userManager.GetRolesAsync(user);
        var response = await _tokenService.GenerateTokensAsync(user.Id, user.Email!, roles);

        // Mark the old token as replaced
        existingToken.ReplacedByToken = response.RefreshToken;
        await _refreshTokenRepository.UpdateAsync(existingToken);

        return response;
    }
}

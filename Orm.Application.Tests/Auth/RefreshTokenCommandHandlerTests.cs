using Microsoft.AspNetCore.Identity;
using Moq;
using Orm.Application.Auth.Commands.RefreshToken;
using Orm.Application.Dtos;
using Orm.Application.Services;
using Orm.Domain.Entities;
using Orm.Domain.Interfaces;

namespace Orm.Application.Tests.Auth;

public class RefreshTokenCommandHandlerTests
{
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock = new();
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new RefreshTokenCommandHandler(
            _refreshTokenRepoMock.Object, _userManagerMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_RotatesTokensSuccessfully()
    {
        // Arrange
        var existingToken = new Domain.Entities.RefreshToken
        {
            Id = 1,
            Token = "old-refresh-token",
            UserId = "user-1",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6)
        };
        var user = new ApplicationUser { Id = "user-1", Email = "test@example.com" };
        var dto = new RefreshTokenDto("old-access", "old-refresh-token");
        var command = new RefreshTokenCommand(dto);

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("old-refresh-token")).ReturnsAsync(existingToken);
        _userManagerMock.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var authResponse = new AuthResponseDto("new-access", "new-refresh", DateTime.UtcNow.AddMinutes(15));
        _tokenServiceMock.Setup(t => t.GenerateTokensAsync(user.Id, user.Email!, It.IsAny<IList<string>>()))
            .ReturnsAsync(authResponse);

        _refreshTokenRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Domain.Entities.RefreshToken>()))
            .ReturnsAsync(existingToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(authResponse, result);
        Assert.NotNull(existingToken.RevokedAt);
        Assert.Equal("new-refresh", existingToken.ReplacedByToken);
        _refreshTokenRepoMock.Verify(r => r.UpdateAsync(existingToken), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsWhenTokenNotFound()
    {
        // Arrange
        var dto = new RefreshTokenDto("access", "nonexistent-token");
        var command = new RefreshTokenCommand(dto);

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("nonexistent-token"))
            .ReturnsAsync((Domain.Entities.RefreshToken?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Invalid or expired", ex.Message);
    }

    [Fact]
    public async Task Handle_ThrowsWhenTokenExpired()
    {
        // Arrange
        var expiredToken = new Domain.Entities.RefreshToken
        {
            Id = 1,
            Token = "expired-token",
            UserId = "user-1",
            CreatedAt = DateTime.UtcNow.AddDays(-10),
            ExpiresAt = DateTime.UtcNow.AddDays(-3)
        };
        var dto = new RefreshTokenDto("access", "expired-token");
        var command = new RefreshTokenCommand(dto);

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("expired-token")).ReturnsAsync(expiredToken);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Invalid or expired", ex.Message);
    }

    [Fact]
    public async Task Handle_ThrowsWhenTokenRevoked()
    {
        // Arrange
        var revokedToken = new Domain.Entities.RefreshToken
        {
            Id = 1,
            Token = "revoked-token",
            UserId = "user-1",
            CreatedAt = DateTime.UtcNow.AddHours(-1),
            ExpiresAt = DateTime.UtcNow.AddDays(6),
            RevokedAt = DateTime.UtcNow.AddMinutes(-30)
        };
        var dto = new RefreshTokenDto("access", "revoked-token");
        var command = new RefreshTokenCommand(dto);

        _refreshTokenRepoMock.Setup(r => r.GetByTokenAsync("revoked-token")).ReturnsAsync(revokedToken);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Invalid or expired", ex.Message);
    }
}

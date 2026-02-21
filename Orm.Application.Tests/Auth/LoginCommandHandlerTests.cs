using Microsoft.AspNetCore.Identity;
using Moq;
using Orm.Application.Auth.Commands.Login;
using Orm.Application.Dtos;
using Orm.Application.Services;
using Orm.Domain.Entities;

namespace Orm.Application.Tests.Auth;

public class LoginCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new LoginCommandHandler(_userManagerMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ReturnsTokens_WhenCredentialsValid()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-1", Email = "bob@example.com", UserName = "bob@example.com" };
        var dto = new LoginDto("bob@example.com", "Password1!");
        var command = new LoginCommand(dto);

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });

        var authResponse = new AuthResponseDto("token", "refresh", DateTime.UtcNow.AddMinutes(15));
        _tokenServiceMock.Setup(t => t.GenerateTokensAsync(user.Id, user.Email, It.IsAny<IList<string>>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(authResponse, result);
    }

    [Fact]
    public async Task Handle_ThrowsWhenUserNotFound()
    {
        // Arrange
        var dto = new LoginDto("nonexistent@example.com", "Password1!");
        var command = new LoginCommand(dto);

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Invalid email or password", ex.Message);
    }

    [Fact]
    public async Task Handle_ThrowsWhenPasswordInvalid()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-1", Email = "bob@example.com" };
        var dto = new LoginDto("bob@example.com", "WrongPassword!");
        var command = new LoginCommand(dto);

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync(user);
        _userManagerMock.Setup(m => m.CheckPasswordAsync(user, dto.Password)).ReturnsAsync(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Invalid email or password", ex.Message);
    }
}

using Microsoft.AspNetCore.Identity;
using Moq;
using Orm.Application.Auth;
using Orm.Application.Auth.Commands.Register;
using Orm.Application.Dtos;
using Orm.Application.Services;
using Orm.Domain.Entities;

namespace Orm.Application.Tests.Auth;

public class RegisterCommandHandlerTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ITokenService> _tokenServiceMock = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new RegisterCommandHandler(_userManagerMock.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var dto = new RegisterDto("alice@example.com", "Password1!", "Alice Smith");
        var command = new RegisterCommand(dto);

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.User))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { AppRoles.User });

        var authResponse = new AuthResponseDto("token", "refresh", DateTime.UtcNow.AddMinutes(15));
        _tokenServiceMock.Setup(t => t.GenerateTokensAsync(It.IsAny<string>(), dto.Email, It.IsAny<IList<string>>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(authResponse, result);
        _userManagerMock.Verify(m => m.CreateAsync(
            It.Is<ApplicationUser>(u => u.Email == dto.Email && u.FullName == dto.FullName),
            dto.Password), Times.Once);
        _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), AppRoles.User), Times.Once);
    }

    [Fact]
    public async Task Handle_ThrowsWhenUserAlreadyExists()
    {
        // Arrange
        var dto = new RegisterDto("existing@example.com", "Password1!", "Existing User");
        var command = new RegisterCommand(dto);

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email))
            .ReturnsAsync(new ApplicationUser { Email = dto.Email });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Contains("already exists", ex.Message);
    }

    [Fact]
    public async Task Handle_ThrowsWhenCreateFails()
    {
        // Arrange
        var dto = new RegisterDto("new@example.com", "weak", "New User");
        var command = new RegisterCommand(dto);

        _userManagerMock.Setup(m => m.FindByEmailAsync(dto.Email)).ReturnsAsync((ApplicationUser?)null);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), dto.Password))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Password too weak", ex.Message);
    }
}

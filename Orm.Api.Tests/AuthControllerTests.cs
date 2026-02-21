using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Orm.Api.Controllers;
using Orm.Application.Auth.Commands.Login;
using Orm.Application.Auth.Commands.RefreshToken;
using Orm.Application.Auth.Commands.Register;
using Orm.Application.Dtos;

namespace Orm.Api.Tests;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Register_Returns200_WithAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterDto("test@example.com", "Password1!", "Test User");
        var authResponse = new AuthResponseDto("access-token", "refresh-token", DateTime.UtcNow.AddMinutes(15));

        _mediatorMock
            .Setup(m => m.Send(It.Is<RegisterCommand>(c => c.RegisterDto == registerDto), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(authResponse, okResult.Value);
    }

    [Fact]
    public async Task Register_Returns400_WhenRegistrationFails()
    {
        // Arrange
        var registerDto = new RegisterDto("existing@example.com", "Password1!", "Test User");

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("A user with this email already exists."));

        // Act
        var result = await _controller.Register(registerDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Login_Returns200_WithAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDto("test@example.com", "Password1!");
        var authResponse = new AuthResponseDto("access-token", "refresh-token", DateTime.UtcNow.AddMinutes(15));

        _mediatorMock
            .Setup(m => m.Send(It.Is<LoginCommand>(c => c.LoginDto == loginDto), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(authResponse, okResult.Value);
    }

    [Fact]
    public async Task Login_Returns401_WhenCredentialsInvalid()
    {
        // Arrange
        var loginDto = new LoginDto("wrong@example.com", "WrongPassword!");

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid email or password."));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task RefreshToken_Returns200_WithNewTokens()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto("old-access", "old-refresh");
        var authResponse = new AuthResponseDto("new-access", "new-refresh", DateTime.UtcNow.AddMinutes(15));

        _mediatorMock
            .Setup(m => m.Send(It.Is<RefreshTokenCommand>(c => c.RefreshTokenDto == refreshTokenDto), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.RefreshToken(refreshTokenDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(authResponse, okResult.Value);
    }

    [Fact]
    public async Task RefreshToken_Returns401_WhenTokenInvalid()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto("old-access", "invalid-refresh");

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid or expired refresh token."));

        // Act
        var result = await _controller.RefreshToken(refreshTokenDto);

        // Assert
        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}

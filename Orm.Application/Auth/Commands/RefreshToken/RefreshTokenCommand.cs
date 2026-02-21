using MediatR;
using Orm.Application.Dtos;

namespace Orm.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(RefreshTokenDto RefreshTokenDto) : IRequest<AuthResponseDto>;

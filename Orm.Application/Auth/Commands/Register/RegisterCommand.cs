using MediatR;
using Orm.Application.Dtos;

namespace Orm.Application.Auth.Commands.Register;

public record RegisterCommand(RegisterDto RegisterDto) : IRequest<AuthResponseDto>;

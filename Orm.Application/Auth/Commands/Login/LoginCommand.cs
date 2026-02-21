using MediatR;
using Orm.Application.Dtos;

namespace Orm.Application.Auth.Commands.Login;

public record LoginCommand(LoginDto LoginDto) : IRequest<AuthResponseDto>;

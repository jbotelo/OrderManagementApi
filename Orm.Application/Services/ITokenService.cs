using Orm.Application.Dtos;

namespace Orm.Application.Services;

public interface ITokenService
{
    Task<AuthResponseDto> GenerateTokensAsync(string userId, string email, IList<string> roles);
}

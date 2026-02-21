namespace Orm.Application.Dtos;

public record AuthResponseDto(string AccessToken, string RefreshToken, DateTime AccessTokenExpiration);

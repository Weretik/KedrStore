namespace Identity.Application.Features.Auth.SessionLogin.DTOs;

public sealed record SessionLoginResult(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);

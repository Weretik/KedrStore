namespace Identity.Application.Features.Auth.Commands.SessionLogin;

public sealed record SessionLoginResult(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);

namespace Identity.Application.Features.Auth.Commands.SessionRefresh;

public sealed record SessionRefreshResult(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);

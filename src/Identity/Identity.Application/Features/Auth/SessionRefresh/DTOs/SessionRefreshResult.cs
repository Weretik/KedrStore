namespace Identity.Application.Features.Auth.SessionRefresh.DTOs;

public sealed record SessionRefreshResult(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);

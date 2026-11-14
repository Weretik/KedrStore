namespace Identity.Api.Endpoints.Auth.Shared;

public sealed record MobileTokenResponse(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);

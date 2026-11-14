namespace Identity.Api.Endpoints.Auth;

public sealed record MobileTokenResponse(
    string TokenType,
    string AccessToken,
    int ExpiresIn,
    string RefreshToken);

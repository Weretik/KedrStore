namespace Identity.Api.Contracts.Auth;

public sealed record SessionTokenResponse(
    string TokenType,
    string AccessToken,
    int ExpiresIn);

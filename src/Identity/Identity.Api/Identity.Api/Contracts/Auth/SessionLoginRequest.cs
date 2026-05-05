namespace Identity.Api.Contracts.Auth;

public sealed record SessionLoginRequest(
    string Email,
    string Password);

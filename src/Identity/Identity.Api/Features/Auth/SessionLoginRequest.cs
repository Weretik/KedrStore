namespace Identity.Api.Features.Auth;

public sealed record SessionLoginRequest(
    string Email,
    string Password);

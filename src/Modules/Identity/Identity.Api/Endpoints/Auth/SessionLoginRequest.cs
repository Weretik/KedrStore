namespace Identity.Api.Endpoints.Auth;

public sealed record SessionLoginRequest(
    string Email,
    string Password);

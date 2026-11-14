namespace Identity.Api.Endpoints.Auth.Shared;

public sealed record SessionLoginRequest(
    string Email,
    string Password);

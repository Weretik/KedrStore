namespace Identity.Application.Features.Auth.Commands.SessionLogin;

public sealed record SessionLoginCommandRequest(
    string Email,
    string Password);

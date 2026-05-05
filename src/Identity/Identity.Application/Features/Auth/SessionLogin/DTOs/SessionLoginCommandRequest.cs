namespace Identity.Application.Features.Auth.SessionLogin.DTOs;

public sealed record SessionLoginCommandRequest(
    string Email,
    string Password);

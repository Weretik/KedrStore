namespace Identity.Application.Features.Auth.SessionMe.DTOs;

public sealed record SessionMeResult(
    string UserId,
    string Email,
    IReadOnlyList<string> Roles);

namespace Identity.Application.Features.Auth.Queries.SessionMe;

public sealed record SessionMeResult(
    string UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    string? AvatarUrl = null);

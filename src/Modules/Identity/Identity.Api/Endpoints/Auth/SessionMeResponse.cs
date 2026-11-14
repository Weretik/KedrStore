using Identity.Application.Features.Auth.Shared;
using Identity.Application.Features.Auth.Queries.SessionMe;

namespace Identity.Api.Endpoints.Auth;

public sealed record SessionMeResponse(
    string UserId,
    string Email,
    string FullName,
    IReadOnlyList<string> Roles,
    string? AvatarUrl)
{
    public static SessionMeResponse From(SessionMeResult result) =>
        new(result.UserId, result.Email, result.FullName, result.Roles, result.AvatarUrl);
}

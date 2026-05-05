namespace Identity.Api.Contracts.Auth;

public sealed record SessionMeResponse(
    string UserId,
    string Email,
    IReadOnlyList<string> Roles);

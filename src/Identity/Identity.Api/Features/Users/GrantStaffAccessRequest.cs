namespace Identity.Api.Features.Users;

public sealed record GrantStaffAccessRequest(
    string FullName,
    string Role,
    string Password);

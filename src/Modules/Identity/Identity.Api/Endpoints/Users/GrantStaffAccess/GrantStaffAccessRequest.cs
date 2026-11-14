namespace Identity.Api.Endpoints.Users.GrantStaffAccess;

public sealed record GrantStaffAccessRequest(
    string FullName,
    string Role,
    string Password);

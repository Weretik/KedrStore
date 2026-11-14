namespace Identity.Api.Endpoints.Users;

public sealed record GrantStaffAccessRequest(
    string FullName,
    string Role,
    string Password);

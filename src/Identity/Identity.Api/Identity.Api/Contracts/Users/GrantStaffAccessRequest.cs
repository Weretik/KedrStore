namespace Identity.Api.Contracts.Users;

public sealed record GrantStaffAccessRequest(
    string FullName,
    string Role,
    string Password);

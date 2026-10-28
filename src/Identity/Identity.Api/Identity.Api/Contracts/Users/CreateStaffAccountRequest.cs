namespace Identity.Api.Contracts.Users;

public sealed record CreateStaffAccountRequest(
    string Email,
    string FullName,
    string Role,
    string Password);

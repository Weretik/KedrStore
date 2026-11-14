namespace Identity.Api.Endpoints.Users;

public sealed record CreateStaffAccountRequest(
    string Email,
    string FullName,
    string Role,
    string Password);

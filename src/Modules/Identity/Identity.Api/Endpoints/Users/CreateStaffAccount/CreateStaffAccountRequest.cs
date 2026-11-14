namespace Identity.Api.Endpoints.Users.CreateStaffAccount;

public sealed record CreateStaffAccountRequest(
    string Email,
    string FullName,
    string Role,
    string Password);

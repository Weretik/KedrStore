namespace Identity.Api.Features.Users;

public sealed record CreateStaffAccountRequest(
    string Email,
    string FullName,
    string Role,
    string Password);

namespace Identity.Application.Features.Users.Shared;

public sealed record StaffAccountDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsBlocked,
    string? AvatarUrl);

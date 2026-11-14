using Identity.Application.Features.Users.Shared;

namespace Identity.Api.Endpoints.Users;

public sealed record StaffAccountResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool IsBlocked,
    string? AvatarUrl)
{
    public static StaffAccountResponse From(StaffAccountDto dto) =>
        new(dto.Id, dto.Email, dto.FullName, dto.Role, dto.IsBlocked, dto.AvatarUrl);
}

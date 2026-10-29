using Identity.Application.Features.Profile.Shared;

namespace Identity.Api.Contracts.Profile;

public sealed record ProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string? AvatarUrl)
{
    public static ProfileResponse From(ProfileDto dto) =>
        new(dto.Id, dto.Email, dto.FullName, dto.Role, dto.AvatarUrl);
}

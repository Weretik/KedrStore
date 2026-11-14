namespace Identity.Application.Features.Profile.Shared;

public sealed record ProfileDto(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    string? AvatarUrl);

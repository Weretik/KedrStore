using Identity.Application.Features.Users.Shared;

namespace Identity.Api.Endpoints.Users;

/// <summary>An existing account without staff access; <c>IsCustomer</c> marks a site/1C customer.</summary>
public sealed record StaffCandidateResponse(
    Guid Id,
    string Email,
    string FullName,
    bool IsCustomer)
{
    public static StaffCandidateResponse From(StaffCandidateDto dto) =>
        new(dto.Id, dto.Email, dto.FullName, dto.IsCustomer);
}

namespace Identity.Application.Features.Users.Shared;

/// <summary>An existing account without a staff role (usually a customer) that can be given staff access.</summary>
public sealed record StaffCandidateDto(
    Guid Id,
    string Email,
    string FullName,
    bool IsCustomer);

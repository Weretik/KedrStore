using Content.Application.Features.Requisites.Shared;

namespace Content.Api.Endpoints.Requisites;

/// <summary>Company requisites for the site and the admin; the IBAN has no spaces, the site groups it.</summary>
public sealed record CompanyRequisitesResponse(
    string LegalName,
    string Code,
    string RegistrationExtractNumber,
    string Iban,
    string BankName,
    string BankCode,
    DateTimeOffset UpdatedAt);

/// <summary>All fields are replaced at once; spaces in the IBAN are removed.</summary>
public sealed record SaveCompanyRequisitesRequest(
    string LegalName,
    string Code,
    string RegistrationExtractNumber,
    string Iban,
    string BankName,
    string BankCode);

/// <summary>One changed field: its API name (<c>iban</c>, <c>bankCode</c> …) and both values.</summary>
public sealed record RequisitesFieldChangeResponse(string Field, string OldValue, string NewValue);

/// <summary>One saved change: when, who (email at that time), and what changed.</summary>
public sealed record CompanyRequisitesChangeResponse(
    long Id,
    DateTimeOffset ChangedAt,
    string ChangedBy,
    IReadOnlyList<RequisitesFieldChangeResponse> Changes);

internal static class CompanyRequisitesResponseMapper
{
    public static CompanyRequisitesResponse ToResponse(CompanyRequisitesDetails details) => new(
        details.LegalName, details.Code, details.RegistrationExtractNumber, details.Iban, details.BankName,
        details.BankCode, details.UpdatedAt);

    public static CompanyRequisitesInput ToInput(SaveCompanyRequisitesRequest request) => new(
        request.LegalName, request.Code, request.RegistrationExtractNumber, request.Iban, request.BankName,
        request.BankCode);

    public static IReadOnlyList<CompanyRequisitesChangeResponse> ToResponse(
        IReadOnlyList<CompanyRequisitesChangeDetails> history) => history
        .Select(entry => new CompanyRequisitesChangeResponse(entry.Id, entry.ChangedAt, entry.ChangedByName,
            entry.Changes.Select(item => new RequisitesFieldChangeResponse(item.Field, item.OldValue, item.NewValue))
                .ToList()))
        .ToList();
}

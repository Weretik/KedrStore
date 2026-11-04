namespace Content.Application.Features.Requisites.Common;

/// <summary>Admin input; all fields are replaced at once.</summary>
public sealed record CompanyRequisitesInput(
    string LegalName,
    string Code,
    string RegistrationExtractNumber,
    string Iban,
    string BankName,
    string BankCode);

public sealed record CompanyRequisitesDetails(
    string LegalName,
    string Code,
    string RegistrationExtractNumber,
    string Iban,
    string BankName,
    string BankCode,
    DateTimeOffset UpdatedAt);

public sealed record RequisitesFieldChangeDetails(string Field, string OldValue, string NewValue);

public sealed record CompanyRequisitesChangeDetails(
    long Id,
    DateTimeOffset ChangedAt,
    string ChangedByName,
    IReadOnlyList<RequisitesFieldChangeDetails> Changes);

internal static class CompanyRequisitesMapper
{
    public static CompanyRequisitesDetails ToDetails(CompanyRequisites requisites) => new(
        requisites.LegalName, requisites.Code, requisites.RegistrationExtractNumber, requisites.Iban,
        requisites.BankName, requisites.BankCode, requisites.UpdatedAt);

    public static CompanyRequisitesValues ToValues(CompanyRequisitesInput input) => new(
        input.LegalName, input.Code, input.RegistrationExtractNumber, input.Iban, input.BankName, input.BankCode);

    public static CompanyRequisitesChangeDetails ToDetails(CompanyRequisitesChange change) => new(
        change.Id, change.ChangedAt, change.ChangedByName,
        change.Changes.Select(item => new RequisitesFieldChangeDetails(
            ToCamelCase(item.Field), item.OldValue, item.NewValue)).ToList());

    /// <summary>Field names as the API spells them (<c>bankCode</c>), so the admin can label them.</summary>
    private static string ToCamelCase(string field) =>
        field.Length == 0 ? field : char.ToLowerInvariant(field[0]) + field[1..];
}

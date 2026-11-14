namespace Content.Application.Features.Documents.Shared;

internal sealed class LegalDocumentVersionByStatusSpec : SingleResultSpecification<LegalDocumentVersion>
{
    public LegalDocumentVersionByStatusSpec(LegalDocumentKind kind, LegalDocumentStatus status) =>
        Query.Where(v => v.Kind == kind && v.Status == status);
}

/// <summary>Numbers already given to versions of a kind.</summary>
internal sealed class LegalDocumentNumbersSpec : Specification<LegalDocumentVersion, int>
{
    public LegalDocumentNumbersSpec(LegalDocumentKind kind) =>
        Query.Where(v => v.Kind == kind && v.Number != null).Select(v => v.Number!.Value);
}

using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.GetVersions;

public sealed record GetLegalDocumentVersionsQuery(LegalDocumentKind Kind)
    : IQuery<Result<IReadOnlyList<LegalDocumentVersionSummary>>>;

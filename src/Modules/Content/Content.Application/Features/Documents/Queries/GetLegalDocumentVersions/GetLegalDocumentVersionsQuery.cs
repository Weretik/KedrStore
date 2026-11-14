using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Queries.GetLegalDocumentVersions;

public sealed record GetLegalDocumentVersionsQuery(string Kind)
    : IQuery<Result<IReadOnlyList<LegalDocumentVersionSummary>>>;

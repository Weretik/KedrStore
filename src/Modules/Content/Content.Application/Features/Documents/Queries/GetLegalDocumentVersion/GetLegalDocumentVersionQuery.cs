using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Queries.GetLegalDocumentVersion;

public sealed record GetLegalDocumentVersionQuery(long Id) : IQuery<Result<LegalDocumentVersionDetails>>;

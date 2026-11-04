using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Public.GetPublished;

public sealed record GetPublishedLegalDocumentQuery(LegalDocumentKind Kind, string Language)
    : IQuery<Result<PublishedLegalDocument>>;

using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Queries.GetPublishedLegalDocument;

public sealed record GetPublishedLegalDocumentQuery(string Kind, string Language)
    : IQuery<Result<PublishedLegalDocument>>;

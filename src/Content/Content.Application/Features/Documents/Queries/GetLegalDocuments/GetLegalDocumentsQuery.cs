using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Queries.GetLegalDocuments;

public sealed record GetLegalDocumentsQuery : IQuery<Result<IReadOnlyList<LegalDocumentOverview>>>;

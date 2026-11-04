using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.GetList;

public sealed record GetLegalDocumentsQuery : IQuery<Result<IReadOnlyList<LegalDocumentOverview>>>;

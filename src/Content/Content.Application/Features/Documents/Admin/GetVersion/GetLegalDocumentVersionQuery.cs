using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.GetVersion;

public sealed record GetLegalDocumentVersionQuery(long Id) : IQuery<Result<LegalDocumentVersionDetails>>;

using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.GetVersion;

public sealed class GetLegalDocumentVersionQueryHandler(IReadContentDbContext db)
    : IQueryHandler<GetLegalDocumentVersionQuery, Result<LegalDocumentVersionDetails>>
{
    public async ValueTask<Result<LegalDocumentVersionDetails>> Handle(GetLegalDocumentVersionQuery query,
        CancellationToken cancellationToken)
    {
        var version = await db.LegalDocumentVersions.AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == query.Id, cancellationToken);
        return version is null ? Result.NotFound() : Result.Success(LegalDocumentMapper.ToDetails(version));
    }
}

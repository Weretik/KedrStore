using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.GetVersions;

/// <summary>All versions of a kind: the draft first, then published and archived ones, newest number first.</summary>
public sealed class GetLegalDocumentVersionsQueryHandler(IReadContentDbContext db)
    : IQueryHandler<GetLegalDocumentVersionsQuery, Result<IReadOnlyList<LegalDocumentVersionSummary>>>
{
    public async ValueTask<Result<IReadOnlyList<LegalDocumentVersionSummary>>> Handle(
        GetLegalDocumentVersionsQuery query, CancellationToken cancellationToken)
    {
        IReadOnlyList<LegalDocumentVersionSummary> versions = await db.LegalDocumentVersions.AsNoTracking()
            .Where(v => v.Kind == query.Kind)
            .OrderBy(v => v.Status == LegalDocumentStatus.Draft ? 0 : 1)
            .ThenByDescending(v => v.Number)
            .Select(v => new LegalDocumentVersionSummary(v.Id, v.Status, v.Number, v.EffectiveFrom, v.UpdatedAt,
                v.UpdatedBy, v.PublishedAt, v.PublishedBy))
            .ToListAsync(cancellationToken);
        return Result.Success(versions);
    }
}

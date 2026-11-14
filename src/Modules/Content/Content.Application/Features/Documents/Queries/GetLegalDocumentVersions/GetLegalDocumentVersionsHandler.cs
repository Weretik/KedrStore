using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Queries.GetLegalDocumentVersions;

/// <summary>All versions of a kind: the draft first, then published and archived ones, newest number first; NotFound for an unknown kind.</summary>
public sealed class GetLegalDocumentVersionsHandler(IReadContentDbContext db)
    : IQueryHandler<GetLegalDocumentVersionsQuery, Result<IReadOnlyList<LegalDocumentVersionSummary>>>
{
    public async ValueTask<Result<IReadOnlyList<LegalDocumentVersionSummary>>> Handle(
        GetLegalDocumentVersionsQuery query, CancellationToken cancellationToken)
    {
        if (!LegalDocumentKinds.TryParse(query.Kind, out var kind))
            return Result.NotFound();

        var versions = await db.LegalDocumentVersions.AsNoTracking()
            .Where(v => v.Kind == kind)
            .OrderBy(v => v.Status == LegalDocumentStatus.Draft ? 0 : 1)
            .ThenByDescending(v => v.Number)
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<LegalDocumentVersionSummary>>(
            versions.Select(LegalDocumentMapper.ToSummary).ToList());
    }
}

using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Admin.GetList;

/// <summary>Every document kind, with its published version and its draft when they exist.</summary>
public sealed class GetLegalDocumentsQueryHandler(IReadContentDbContext db)
    : IQueryHandler<GetLegalDocumentsQuery, Result<IReadOnlyList<LegalDocumentOverview>>>
{
    public async ValueTask<Result<IReadOnlyList<LegalDocumentOverview>>> Handle(GetLegalDocumentsQuery query,
        CancellationToken cancellationToken)
    {
        var current = await db.LegalDocumentVersions.AsNoTracking()
            .Where(v => v.Status != LegalDocumentStatus.Archived)
            .Select(v => new
            {
                v.Kind,
                Summary = new LegalDocumentVersionSummary(v.Id, v.Status, v.Number, v.EffectiveFrom, v.UpdatedAt,
                    v.UpdatedBy, v.PublishedAt, v.PublishedBy)
            })
            .ToListAsync(cancellationToken);

        LegalDocumentVersionSummary? Find(LegalDocumentKind kind, LegalDocumentStatus status) =>
            current.FirstOrDefault(v => v.Kind == kind && v.Summary.Status == status)?.Summary;

        IReadOnlyList<LegalDocumentOverview> overview = LegalDocumentKinds.All
            .Select(kind => new LegalDocumentOverview(kind, Find(kind, LegalDocumentStatus.Published),
                Find(kind, LegalDocumentStatus.Draft)))
            .ToList();
        return Result.Success(overview);
    }
}

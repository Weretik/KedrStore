using Content.Application.Features.Documents.Shared;

namespace Content.Application.Features.Documents.Queries.GetLegalDocuments;

/// <summary>Every document kind, with its published version and its draft when they exist.</summary>
public sealed class GetLegalDocumentsHandler(IReadContentDbContext db)
    : IQueryHandler<GetLegalDocumentsQuery, Result<IReadOnlyList<LegalDocumentOverview>>>
{
    public async ValueTask<Result<IReadOnlyList<LegalDocumentOverview>>> Handle(GetLegalDocumentsQuery query,
        CancellationToken cancellationToken)
    {
        var current = await db.LegalDocumentVersions.AsNoTracking()
            .Where(v => v.Status != LegalDocumentStatus.Archived)
            .ToListAsync(cancellationToken);

        LegalDocumentVersionSummary? Find(LegalDocumentKind kind, LegalDocumentStatus status) =>
            current.FirstOrDefault(v => v.Kind == kind && v.Status == status) is { } version
                ? LegalDocumentMapper.ToSummary(version)
                : null;

        IReadOnlyList<LegalDocumentOverview> overview = LegalDocumentKinds.All
            .Select(kind => new LegalDocumentOverview(LegalDocumentKinds.ToSlug(kind), Find(kind, LegalDocumentStatus.Published),
                Find(kind, LegalDocumentStatus.Draft)))
            .ToList();
        return Result.Success(overview);
    }
}

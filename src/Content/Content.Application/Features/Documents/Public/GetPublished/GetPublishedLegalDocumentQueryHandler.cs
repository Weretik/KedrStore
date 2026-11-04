using Content.Application.Features.Documents.Common;

namespace Content.Application.Features.Documents.Public.GetPublished;

/// <summary>The one published version of a kind in one language; NotFound when nothing is published.</summary>
public sealed class GetPublishedLegalDocumentQueryHandler(IReadContentDbContext db)
    : IQueryHandler<GetPublishedLegalDocumentQuery, Result<PublishedLegalDocument>>
{
    public async ValueTask<Result<PublishedLegalDocument>> Handle(GetPublishedLegalDocumentQuery query,
        CancellationToken cancellationToken)
    {
        var language = query.Language;
        var version = await db.LegalDocumentVersions.AsNoTracking()
            .Where(v => v.Kind == query.Kind && v.Status == LegalDocumentStatus.Published)
            .Select(v => new
            {
                v.Kind, v.Number, v.EffectiveFrom, v.PublishedAt,
                Text = v.Texts.Where(t => t.Language == language)
                    .Select(t => new { t.Title, t.BodyHtml }).FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (version?.Text is null || version.Number is null || version.EffectiveFrom is null ||
            version.PublishedAt is null)
            return Result.NotFound();

        return Result.Success(new PublishedLegalDocument(version.Kind, version.Number.Value,
            version.EffectiveFrom.Value, version.Text.Title, version.Text.BodyHtml, version.PublishedAt.Value));
    }
}

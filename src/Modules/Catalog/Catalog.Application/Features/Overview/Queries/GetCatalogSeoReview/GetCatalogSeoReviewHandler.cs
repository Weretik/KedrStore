using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.SiteModels.Shared.Public;
using Catalog.Contracts.Overview;

namespace Catalog.Application.Features.Overview.Queries.GetCatalogSeoReview;

/// <summary>
/// Which pages the site shows comes from the public model catalogue (shown categories, accepted models with on-site
/// products; up to two minutes old); their texts are read fresh, so a saved text leaves the review at once. A
/// category closed from search and its models are left out (spec catalog/011 R-01).
/// </summary>
public sealed class GetCatalogSeoReviewHandler(IReadCatalogDbContext db, PublicModelCatalogReader reader)
    : IQueryHandler<GetCatalogSeoReviewQuery, Result<IReadOnlyList<SeoReviewItemResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<SeoReviewItemResponse>>> Handle(GetCatalogSeoReviewQuery query,
        CancellationToken cancellationToken)
    {
        var catalog = await reader.ReadAsync(cancellationToken);
        var categories = catalog.Categories.Values.Where(category => !category.IsClosedFromSearch).ToList();
        var categoryIds = categories.Select(category => category.Id).ToList();
        var models = catalog.Models
            .Where(model => model.ModelId is not null && categoryIds.Contains(model.CategoryId))
            .ToList();
        var modelIds = models.Select(model => model.ModelId!.Value).ToList();

        var categoryTexts = (await db.CategoryContents.AsNoTracking()
                .Where(content => categoryIds.Contains(content.CategoryId))
                .Select(content => new { content.CategoryId, content.Language, content.SeoTitle, content.MetaDescription, content.BodyHtml })
                .ToListAsync(cancellationToken))
            .ToDictionary(content => (content.CategoryId, content.Language));
        var modelTexts = (await db.ProductGroupTexts.AsNoTracking()
                .Where(text => modelIds.Contains(text.GroupId))
                .Select(text => new { text.GroupId, text.Language, text.SeoTitle, text.SeoDescription, text.Description })
                .ToListAsync(cancellationToken))
            .ToDictionary(text => (text.GroupId, text.Language));

        var pages = new List<CatalogSeoPage>();
        foreach (var language in CategoryContent.Languages)
        {
            foreach (var category in categories)
            {
                var text = categoryTexts.GetValueOrDefault((category.Id, language));
                pages.Add(new CatalogSeoPage("category", category.Id.Value, category.Name(language), language,
                    text?.SeoTitle, text?.MetaDescription, !string.IsNullOrWhiteSpace(text?.BodyHtml)));
            }

            foreach (var model in models)
            {
                var text = modelTexts.GetValueOrDefault((model.ModelId!.Value, language));
                pages.Add(new CatalogSeoPage("model", model.ModelId.Value.Value, model.Name, language, text?.SeoTitle,
                    text?.SeoDescription, !string.IsNullOrWhiteSpace(text?.Description)));
            }
        }

        return Result.Success(CatalogSeoChecks.Review(pages));
    }
}

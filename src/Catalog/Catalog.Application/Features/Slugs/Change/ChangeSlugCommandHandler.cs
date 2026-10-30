using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Slugs;
using Catalog.Contracts.Slugs;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Slugs.Change;

/// <summary>Admin slug edit (R-010): the old slug goes to history and list projections follow the new slug.</summary>
public sealed class ChangeSlugCommandHandler(
    ICatalogRepository<Product> productRepo,
    ICatalogRepository<ProductCategory> categoryRepo,
    IReadCatalogDbContext catalogDbContext,
    ISlugService slugService)
    : ICommandHandler<ChangeSlugCommand, Result<ChangeSlugResponse>>
{
    public async ValueTask<Result<ChangeSlugResponse>> Handle(ChangeSlugCommand command, CancellationToken cancellationToken)
    {
        var target = await LoadAsync(command.EntityType, command.Id, cancellationToken);
        if (target is null)
            return Result.NotFound();

        var newSlug = command.Request.Regenerate
            ? await GenerateAsync(command.EntityType, command.Id, target.Name, target.Slug, cancellationToken)
            : command.Request.Slug!.Trim();

        if (newSlug == target.Slug)
            return Result.Success(new ChangeSlugResponse(command.Id, newSlug, target.Slug));

        if (await slugService.IsTakenByOtherAsync(command.EntityType, newSlug, command.Id, cancellationToken))
        {
            return Result.Invalid(new ValidationError(
                "slug", "This slug is used by another item now or was used before.", "Catalog.Slug.Taken",
                ValidationSeverity.Error));
        }

        var now = DateTimeOffset.UtcNow;
        await slugService.RecordChangeAsync(command.EntityType, command.Id, target.Slug, newSlug, now, cancellationToken);
        target.Apply(newSlug, now);
        // Both repositories share one DbContext: this saves the entity and its history rows together.
        await productRepo.SaveChangesAsync(cancellationToken);
        await UpdateProjectionsAsync(command.EntityType, command.Id, newSlug, cancellationToken);

        return Result.Success(new ChangeSlugResponse(command.Id, newSlug, target.Slug));
    }

    private async Task<Target?> LoadAsync(CatalogEntityType entityType, int id, CancellationToken cancellationToken)
    {
        if (entityType == CatalogEntityType.Product)
        {
            var product = await productRepo.GetByIdAsync(ProductId.From(id), cancellationToken);
            return product is null ? null : new Target(product.Name, product.ProductSlug, product.ChangeSlug);
        }

        var category = await categoryRepo.GetByIdAsync(ProductCategoryId.From(id), cancellationToken);
        return category is null ? null : new Target(category.Name, category.Slug, category.ChangeSlug);
    }

    private async Task<string> GenerateAsync(CatalogEntityType entityType, int id, string name, string currentSlug,
        CancellationToken cancellationToken)
    {
        var baseSlug = SlugText.FromName(name, entityType);
        for (var number = 1; ; number++)
        {
            var candidate = SlugText.WithSuffix(baseSlug, number);
            if (candidate == currentSlug ||
                !await slugService.IsTakenByOtherAsync(entityType, candidate, id, cancellationToken))
            {
                return candidate;
            }
        }
    }

    private Task UpdateProjectionsAsync(CatalogEntityType entityType, int id, string slug, CancellationToken cancellationToken)
    {
        var projections = catalogDbContext.ProductListProjections;
        return entityType == CatalogEntityType.Product
            ? projections.Where(row => row.ProductId == ProductId.From(id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(row => row.ProductSlug, slug), cancellationToken)
            : projections.Where(row => row.CategoryId == ProductCategoryId.From(id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(row => row.CategorySlug, slug), cancellationToken);
    }

    private sealed record Target(string Name, string Slug, Action<string, DateTimeOffset> Apply);
}

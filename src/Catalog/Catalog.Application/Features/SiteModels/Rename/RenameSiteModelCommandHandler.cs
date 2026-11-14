using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.SiteModels.Common;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Rename;

/// <summary>Renames a model; its slug stays (spec catalog/006 R-12).</summary>
public sealed class RenameSiteModelCommandHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> models,
    SiteModelReader reader,
    TimeProvider clock)
    : ICommandHandler<RenameSiteModelCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(RenameSiteModelCommand command,
        CancellationToken cancellationToken)
    {
        var id = ProductGroupId.Create(command.ModelId);
        var model = await db.ProductGroups.SingleOrDefaultAsync(group => group.Id == id, cancellationToken);
        if (model is null)
            return Result.NotFound();

        if (model.Rename(command.Request.Name, clock.GetUtcNow()))
            await models.SaveChangesAsync(cancellationToken);

        return Result.Success((await reader.ReadAsync(command.ModelId, cancellationToken))!);
    }
}

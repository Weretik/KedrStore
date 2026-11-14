using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Commands.SetSiteModelText;

/// <summary>Saves the description and SEO texts of one language (R-07).</summary>
public sealed class SetSiteModelTextHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroupText> texts,
    SiteModelReader reader,
    TimeProvider clock)
    : ICommandHandler<SetSiteModelTextCommand, Result<SiteModelDto>>
{
    public async ValueTask<Result<SiteModelDto>> Handle(SetSiteModelTextCommand command,
        CancellationToken cancellationToken)
    {
        var modelId = ProductGroupId.Create(command.ModelId);
        if (!await db.ProductGroups.AnyAsync(group => group.Id == modelId, cancellationToken))
            return Result.NotFound();

        var now = clock.GetUtcNow();
        var request = command.Request;
        var text = await db.ProductGroupTexts
            .SingleOrDefaultAsync(row => row.GroupId == modelId && row.Language == command.Language, cancellationToken);
        if (text is null)
        {
            text = ProductGroupText.Create(modelId, command.Language, now);
            text.Update(request.ShortDescription, request.Description, request.SeoTitle, request.SeoDescription, now);
            await texts.AddAsync(text, cancellationToken);
        }
        else if (text.Update(request.ShortDescription, request.Description, request.SeoTitle, request.SeoDescription,
                     now))
        {
            await texts.SaveChangesAsync(cancellationToken);
        }

        return Result.Success((await reader.ReadAsync(command.ModelId, cancellationToken))!);
    }
}

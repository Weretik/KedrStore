using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Groups.Shared;
using Catalog.Contracts.Groups;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.Groups.Commands.SetProductGroupMain;

public sealed class SetProductGroupMainHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> groups,
    ProductGroupReader reader,
    TimeProvider clock)
    : ICommandHandler<SetProductGroupMainCommand, Result<AdminProductGroupDto>>
{
    public async ValueTask<Result<AdminProductGroupDto>> Handle(SetProductGroupMainCommand command,
        CancellationToken cancellationToken)
    {
        var group = await groups.GetByIdAsync(ProductGroupId.Create(command.GroupId), cancellationToken);
        if (group is null)
            return Result.NotFound();

        var productId = ProductId.From(command.Request.ProductId);
        var isMember = await db.ProductGroupMembers
            .AnyAsync(member => member.ProductId == productId && member.GroupId == group.Id, cancellationToken);
        if (!isMember)
            return ProductGroupValidation.Invalid("productId", "The product is not in this group.", "NotMember");

        var onSite = await db.Products
            .AnyAsync(product => product.Id == productId && product.ExportToSite, cancellationToken);
        if (!onSite)
        {
            return ProductGroupValidation.Invalid("productId", "Only a product shown on the site can be main.",
                "MainNotOnSite");
        }

        group.SetMainManually(productId, clock.GetUtcNow());
        await groups.SaveChangesAsync(cancellationToken);

        return Result.Success((await reader.ReadAsync(group.Id, cancellationToken))!);
    }
}

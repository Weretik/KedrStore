using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Groups.Shared;
using Catalog.Contracts.Groups;
using Catalog.Domain.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Groups.Commands.RenameProductGroup;

public sealed class RenameProductGroupCommandHandler(
    ICatalogRepository<ProductGroup> groups,
    ProductGroupReader reader,
    TimeProvider clock)
    : ICommandHandler<RenameProductGroupCommand, Result<AdminProductGroupDto>>
{
    public async ValueTask<Result<AdminProductGroupDto>> Handle(RenameProductGroupCommand command,
        CancellationToken cancellationToken)
    {
        var group = await groups.GetByIdAsync(ProductGroupId.Create(command.GroupId), cancellationToken);
        if (group is null)
            return Result.NotFound();

        if (group.Rename(command.Request.Name, clock.GetUtcNow()))
            await groups.SaveChangesAsync(cancellationToken);

        return Result.Success((await reader.ReadAsync(group.Id, cancellationToken))!);
    }
}

using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Groups.Common;
using Catalog.Contracts.Groups;
using Catalog.Domain.Entities.Groups;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Groups.Rename;

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

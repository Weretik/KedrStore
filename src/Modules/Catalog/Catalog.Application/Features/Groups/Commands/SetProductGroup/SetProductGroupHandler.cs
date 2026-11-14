using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Groups.Shared;
using Catalog.Contracts.Groups;
using Catalog.Domain.Groups;
using Catalog.Domain.Groups.Members;

namespace Catalog.Application.Features.Groups.Commands.SetProductGroup;

public sealed class SetProductGroupHandler(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroupMember> members,
    ProductGroupPins pins,
    ProductGroupBuilder builder,
    ProductGroupReader reader,
    TimeProvider clock)
    : ICommandHandler<SetProductGroupCommand, Result<AdminProductGroupMembershipDto>>
{
    public async ValueTask<Result<AdminProductGroupMembershipDto>> Handle(SetProductGroupCommand command,
        CancellationToken cancellationToken)
    {
        var productId = ProductId.From(command.ProductId);
        var product = await db.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(item => item.Id == productId)
            .Select(item => new { item.CategoryId })
            .FirstOrDefaultAsync(cancellationToken);
        if (product is null)
            return Result.NotFound();

        var now = clock.GetUtcNow();
        if (command.Request.Automatic)
        {
            var row = await db.ProductGroupMembers.FirstOrDefaultAsync(member => member.ProductId == productId,
                cancellationToken);
            if (row is { IsPinned: true })
            {
                row.Unpin(now);
                await members.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            ProductGroupId? groupId = command.Request.GroupId is { } id ? ProductGroupId.Create(id) : null;
            if (groupId is not null)
            {
                var groupCategory = await db.ProductGroups.AsNoTracking()
                    .Where(group => group.Id == groupId)
                    .Select(group => (ProductCategoryId?)group.CategoryId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (groupCategory is null)
                    return ProductGroupValidation.Invalid("groupId", "Групу не знайдено.", "GroupNotFound");
                if (groupCategory != product.CategoryId)
                {
                    return ProductGroupValidation.Invalid("groupId",
                        "Група належить до іншої категорії, ніж товар.", "OtherCategory");
                }
            }

            await pins.PinAsync([productId], groupId, now, cancellationToken);
        }

        await builder.RunAsync(apply: true, cancellationToken);
        return Result.Success((await reader.ReadForProductAsync(productId, cancellationToken))!);
    }
}

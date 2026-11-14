using Catalog.Application.Attributes.Import;
using Catalog.Application.Contracts.Persistence;
using Catalog.Contracts.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.Groups.Common;

/// <summary>Reads a group with its variants and their color, size, and side for the admin.</summary>
public sealed class ProductGroupReader(IReadCatalogDbContext db)
{
    public async Task<AdminProductGroupDto?> ReadAsync(ProductGroupId groupId, CancellationToken cancellationToken)
    {
        var group = await db.ProductGroups.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == groupId, cancellationToken);
        return group is null ? null : await ToDtoAsync(group, cancellationToken);
    }

    public async Task<AdminProductGroupMembershipDto?> ReadForProductAsync(ProductId productId,
        CancellationToken cancellationToken)
    {
        if (!await db.Products.IgnoreQueryFilters().AnyAsync(product => product.Id == productId, cancellationToken))
            return null;

        var row = await db.ProductGroupMembers.AsNoTracking()
            .FirstOrDefaultAsync(member => member.ProductId == productId, cancellationToken);
        var group = row?.GroupId is { } groupId ? await ReadAsync(groupId, cancellationToken) : null;
        return new AdminProductGroupMembershipDto(productId.Value, row?.IsPinned ?? false, group);
    }

    private async Task<AdminProductGroupDto> ToDtoAsync(ProductGroup group, CancellationToken cancellationToken)
    {
        var rows = await db.ProductGroupMembers.AsNoTracking()
            .Where(member => member.GroupId == group.Id)
            .ToListAsync(cancellationToken);
        var ids = rows.Select(row => row.ProductId).ToList();

        var products = await db.Products.IgnoreQueryFilters().AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .Select(product => new
            {
                product.Id, product.Name, product.ProductSlug, product.ExportToSite, product.IsDeleted,
                product.Photo, product.Stock
            })
            .ToListAsync(cancellationToken);

        var axisCodes = new[] { AttributeImportCatalog.Color, AttributeImportCatalog.Size, AttributeImportCatalog.Side };
        var values = (await (
                from value in db.ProductAttributeValues.AsNoTracking()
                join attribute in db.Attributes.AsNoTracking() on value.AttributeId equals attribute.Id
                join name in db.AttributeValues.AsNoTracking() on value.ValueId equals (AttributeValueId?)name.Id
                where ids.Contains(value.ProductId) && axisCodes.Contains(attribute.Code)
                select new { value.ProductId, attribute.Code, value.Position, name.NameUk })
            .ToListAsync(cancellationToken))
            .ToLookup(value => value.ProductId);

        var sameVariants = (await db.ProductAttributeReviews.AsNoTracking()
                .Where(review => ids.Contains(review.ProductId))
                .ToListAsync(cancellationToken))
            .Where(review => review.Reasons.Any(reason => reason.Code == AttributeReviewReason.SameVariants))
            .Select(review => review.ProductId)
            .ToHashSet();

        string? Single(ProductId id, string code)
            => values[id].Where(value => value.Code == code).OrderBy(value => value.Position)
                .Select(value => value.NameUk).FirstOrDefault();

        var pinned = rows.ToDictionary(row => row.ProductId, row => row.IsPinned);
        var members = products
            .OrderByDescending(product => product.Id == group.MainProductId)
            .ThenBy(product => product.Id.Value)
            .Select(product => new AdminProductGroupMemberDto(
                product.Id.Value, product.Name, product.ProductSlug, product.ExportToSite, product.IsDeleted,
                product.Photo, product.Stock, pinned[product.Id], product.Id == group.MainProductId,
                values[product.Id].Where(value => value.Code == AttributeImportCatalog.Color)
                    .OrderBy(value => value.Position).Select(value => value.NameUk).ToList(),
                Single(product.Id, AttributeImportCatalog.Size),
                Single(product.Id, AttributeImportCatalog.Side),
                sameVariants.Contains(product.Id)))
            .ToList();

        return new AdminProductGroupDto(group.Id.Value, group.CategoryId.Value, group.Name, group.Slug,
            group.MainProductId?.Value, group.IsMainManual, group.IsCreatedManually, members);
    }
}

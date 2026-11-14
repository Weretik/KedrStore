using Catalog.Application.Features.Attributes.Shared.Import;
using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Features.Slugs.Shared;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.Groups.Shared;

/// <summary>
/// Builds variant groups from the stored base names (spec catalog/006, phase 2). Products with the same base name in
/// one category form a group when there are at least two of them; an on-site product without such a group gets a
/// one-product group (spec catalog/007 R-02). Every group it creates is a proposed site model; it never changes an
/// accepted model (R-03), whose products are pinned. Rows a person pinned are never moved; No name
/// products and deleted products never join automatically. The main product stays while it is on the site;
/// otherwise the best product is chosen: on the site, with a photo, in stock, then the smallest 1C code.
/// Products a switcher cannot tell apart get the review reason «Однакові варіанти». Idempotent.
/// </summary>
public sealed class ProductGroupBuilder(
    IReadCatalogDbContext db,
    ICatalogRepository<ProductGroup> groups,
    ICatalogRepository<ProductGroupMember> members,
    ISlugService slugs,
    TimeProvider clock)
{
    public const int MaxNamedMatches = 10;

    public async Task<ProductGroupBuildReport> RunAsync(bool apply, CancellationToken cancellationToken)
    {
        var report = new ProductGroupBuildReport();
        var now = clock.GetUtcNow();

        var products = (await db.Products.IgnoreQueryFilters().AsNoTracking()
                .Select(product => new ProductFacts(product.Id, product.CategoryId, product.ExportToSite,
                    product.IsDeleted, product.Photo != "", product.Stock > 0))
                .ToListAsync(cancellationToken))
            .ToDictionary(product => product.Id);
        var reviews = await db.ProductAttributeReviews.ToListAsync(cancellationToken);
        var existingGroups = await db.ProductGroups.ToListAsync(cancellationToken);
        var rows = (await db.ProductGroupMembers.ToListAsync(cancellationToken)).ToDictionary(row => row.ProductId);
        var groupById = existingGroups.ToDictionary(group => group.Id);
        var autoGroupByKey = existingGroups.Where(group => group.GroupKey is not null)
            .ToDictionary(group => (group.CategoryId, group.GroupKey!));

        // Candidates: automatic products by group key, in product order so names and slugs are stable.
        var candidates = reviews
            .Where(review => products.TryGetValue(review.ProductId, out var product) && !product.IsDeleted &&
                             review.Reasons.All(reason => reason.Code != AttributeReviewReason.NoName) &&
                             !(rows.TryGetValue(review.ProductId, out var row) && row.IsPinned))
            .OrderBy(review => review.ProductId.Value)
            .GroupBy(review => (products[review.ProductId].CategoryId, Key: ProductGroupKey.From(review.BaseName)))
            .ToDictionary(group => group.Key, group => group.ToList());
        var pinnedCount = rows.Values.Where(row => row.IsPinned && row.GroupId is not null)
            .GroupBy(row => row.GroupId!.Value)
            .ToDictionary(group => group.Key, group => group.Count());

        // 1. Groups: one per key with at least two products (pinned products of an existing group count too).
        var targetByKey = new Dictionary<(ProductCategoryId, string), ProductGroup>();
        var createdGroups = new List<ProductGroup>();
        SlugAllocator? allocator = null;
        foreach (var (key, keyReviews) in candidates)
        {
            autoGroupByKey.TryGetValue(key, out var existing);
            var pinned = existing is null ? 0 : pinnedCount.GetValueOrDefault(existing.Id);
            var aloneOnSite = keyReviews.Count == 1 && pinned == 0 && products[keyReviews[0].ProductId].ExportToSite;
            if (keyReviews.Count + pinned < 2 && !aloneOnSite)
                continue;

            if (existing is null)
            {
                allocator ??= await slugs.LoadAllocatorAsync(CatalogEntityType.ProductGroup, cancellationToken);
                var name = ProductGroupKey.DisplayName(keyReviews[0].BaseName);
                existing = ProductGroup.CreateAutomatic(key.CategoryId, name, key.Key, allocator.Allocate(name), now);
                createdGroups.Add(existing);
                report.Groups.Created++;
            }

            targetByKey[key] = existing;
        }

        if (apply && createdGroups.Count > 0)
            await groups.AddRangeAsync(createdGroups, cancellationToken);

        // 2. Rows: candidates follow their key; pinned rows stay; other rows leave their group.
        var final = new Dictionary<ProductId, ProductGroup>();
        var createdRows = new List<ProductGroupMember>();
        var removedRows = new List<ProductGroupMember>();
        var candidateIds = new HashSet<ProductId>();
        foreach (var (key, keyReviews) in candidates)
        {
            targetByKey.TryGetValue(key, out var target);
            foreach (var review in keyReviews)
            {
                candidateIds.Add(review.ProductId);
                rows.TryGetValue(review.ProductId, out var row);
                if (target is null)
                {
                    if (row is not null)
                        removedRows.Add(row);
                    continue;
                }

                final[review.ProductId] = target;
                if (row is null)
                {
                    if (apply)
                        createdRows.Add(ProductGroupMember.CreateAutomatic(review.ProductId, target.Id, now));
                    report.Members.Created++;
                }
                else if (row.GroupId == target.Id && !createdGroups.Contains(target))
                {
                    report.Members.Unchanged++;
                }
                else
                {
                    if (apply)
                        row.MoveAutomatically(target.Id, now);
                    report.Members.Updated++;
                }
            }
        }

        foreach (var row in rows.Values)
        {
            if (row.IsPinned)
            {
                report.Members.Skipped++;
                if (row.GroupId is { } groupId && groupById.TryGetValue(groupId, out var pinnedGroup))
                    final[row.ProductId] = pinnedGroup;
            }
            else if (!candidateIds.Contains(row.ProductId))
            {
                // Deleted, No name, or without a base name: automation takes it out of its group.
                removedRows.Add(row);
            }
        }

        report.Members.Removed = removedRows.Count;
        if (apply)
        {
            if (removedRows.Count > 0)
                await members.DeleteRangeAsync(removedRows, cancellationToken);
            if (createdRows.Count > 0)
                await members.AddRangeAsync(createdRows, cancellationToken);
        }

        // 3. Proposed groups left without products disappear; accepted models stay.
        var memberLists = final.GroupBy(pair => pair.Value)
            .ToDictionary(group => group.Key, group => group.Select(pair => products[pair.Key]).ToList());
        var removedGroups = existingGroups
            .Where(group => !group.IsAccepted && !memberLists.ContainsKey(group))
            .ToList();
        report.Groups.Removed = removedGroups.Count;
        if (apply && removedGroups.Count > 0)
            await groups.DeleteRangeAsync(removedGroups, cancellationToken);

        // 4. Main product of each group.
        foreach (var group in existingGroups.Except(removedGroups).Concat(createdGroups))
        {
            var groupProducts = memberLists.GetValueOrDefault(group) ?? [];
            var changed = KeepsMain(group, groupProducts)
                ? false
                : group.SetMainAutomatically(PickMain(groupProducts), now);

            if (createdGroups.Contains(group))
                continue;
            if (group.IsAccepted && !changed)
                report.Groups.Skipped++;
            else if (changed)
                report.Groups.Updated++;
            else
                report.Groups.Unchanged++;
        }

        report.GroupedProducts = final.Count;

        // 5. «Однакові варіанти»: products of one group that color, size, and side do not tell apart.
        var sameVariants = await FindSameVariantsAsync(memberLists, cancellationToken);
        report.ProductsWithSameVariants = sameVariants.Count;
        report.GroupsWithSameVariants = memberLists.Keys.Count(group =>
            memberLists[group].Any(product => sameVariants.ContainsKey(product.Id)));
        foreach (var review in reviews)
        {
            var text = sameVariants.TryGetValue(review.ProductId, out var matches)
                ? "Не відрізняється кольором, розміром і стороною від: " +
                  string.Join(", ", matches.Take(MaxNamedMatches).Select(id => $"#{id.Value}"))
                : null;
            if (review.SetSameVariants(text, now))
                report.ReviewsChanged++;
        }

        if (apply)
            await groups.SaveChangesAsync(cancellationToken);

        return report;
    }

    private static bool KeepsMain(ProductGroup group, IReadOnlyList<ProductFacts> groupProducts)
        => group.MainProductId is { } mainId &&
           groupProducts.Any(product => product.Id == mainId && product.ExportToSite && !product.IsDeleted);

    private static ProductId? PickMain(IReadOnlyList<ProductFacts> groupProducts)
        => groupProducts
            .Where(product => !product.IsDeleted)
            .OrderByDescending(product => product.ExportToSite)
            .ThenByDescending(product => product.HasPhoto)
            .ThenByDescending(product => product.InStock)
            .ThenBy(product => product.Id.Value)
            .Select(product => (ProductId?)product.Id)
            .FirstOrDefault();

    /// <summary>For each product that shares its color, second color, size, and side with others in its group: those others.</summary>
    private async Task<Dictionary<ProductId, List<ProductId>>> FindSameVariantsAsync(
        Dictionary<ProductGroup, List<ProductFacts>> memberLists, CancellationToken cancellationToken)
    {
        var axisCodes = new[] { AttributeImportCatalog.Color, AttributeImportCatalog.Size, AttributeImportCatalog.Side };
        var axes = await db.Attributes.AsNoTracking()
            .Where(attribute => axisCodes.Contains(attribute.Code))
            .Select(attribute => new { attribute.Id, attribute.Code })
            .ToListAsync(cancellationToken);
        var axisIds = axes.Select(axis => axis.Id).ToList();
        var values = (await db.ProductAttributeValues.AsNoTracking()
                .Where(value => axisIds.Contains(value.AttributeId))
                .Select(value => new { value.ProductId, value.AttributeId, value.Position, value.ValueId })
                .ToListAsync(cancellationToken))
            .ToLookup(value => value.ProductId);

        string Signature(ProductId productId)
            => string.Join('|', axes.OrderBy(axis => axis.Code, StringComparer.Ordinal).Select(axis =>
                string.Join(',', values[productId]
                    .Where(value => value.AttributeId == axis.Id)
                    .OrderBy(value => value.Position)
                    .Select(value => value.ValueId?.Value.ToString() ?? "?"))));

        var result = new Dictionary<ProductId, List<ProductId>>();
        foreach (var groupProducts in memberLists.Values)
        {
            foreach (var same in groupProducts.Where(product => !product.IsDeleted)
                         .GroupBy(product => Signature(product.Id))
                         .Where(same => same.Count() > 1))
            {
                var ids = same.Select(product => product.Id).OrderBy(id => id.Value).ToList();
                foreach (var id in ids)
                    result[id] = ids.Where(other => other != id).ToList();
            }
        }

        return result;
    }

    private sealed record ProductFacts(
        ProductId Id, ProductCategoryId CategoryId, bool ExportToSite, bool IsDeleted, bool HasPhoto, bool InStock);
}

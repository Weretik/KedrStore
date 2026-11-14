using Content.Application.Abstractions.Media;

namespace Content.Application.Features.Home.Shared;

public static class HomeAdminBlocks
{
    public static async Task<AdminHomeBlocks> LoadAsync(IReadContentDbContext db, IMediaImageReader images,
        CancellationToken cancellationToken)
    {
        var faq = await db.HomeFaqItems.AsNoTracking()
            .OrderBy(item => item.Position).ThenBy(item => item.Id)
            .Select(item => new AdminHomeFaqItem(item.Id, item.Position, item.QuestionUk, item.QuestionRu,
                item.AnswerUk, item.AnswerRu, item.UpdatedAt))
            .ToListAsync(cancellationToken);
        var brands = await db.HomeBrands.AsNoTracking()
            .OrderBy(brand => brand.Position).ThenBy(brand => brand.Id)
            .ToListAsync(cancellationToken);
        var logos = await images.GetImagesAsync(brands.Select(brand => brand.LogoMediaFileId).ToList(),
            cancellationToken);

        return new AdminHomeBlocks(faq, brands.Select(brand =>
        {
            logos.TryGetValue(brand.LogoMediaFileId, out var logo);
            return new AdminHomeBrand(brand.Id, brand.Position, brand.Name, brand.LogoMediaFileId, logo?.Url,
                logo?.FileName, logo is null, brand.UpdatedAt);
        }).ToList());
    }

    public static async Task<int> NextFaqPositionAsync(IReadContentDbContext db, CancellationToken cancellationToken)
        => await db.HomeFaqItems.AnyAsync(cancellationToken)
            ? await db.HomeFaqItems.MaxAsync(item => item.Position, cancellationToken) + 1
            : 0;

    public static async Task<int> NextBrandPositionAsync(IReadContentDbContext db, CancellationToken cancellationToken)
        => await db.HomeBrands.AnyAsync(cancellationToken)
            ? await db.HomeBrands.MaxAsync(brand => brand.Position, cancellationToken) + 1
            : 0;

    /// <summary>400 when the chosen logo is not in the media library.</summary>
    public static async Task<ValidationError?> CheckLogoAsync(IMediaImageReader images, long mediaFileId,
        CancellationToken cancellationToken)
    {
        var files = await images.GetImagesAsync([mediaFileId], cancellationToken);
        return files.ContainsKey(mediaFileId)
            ? null
            : new ValidationError("logoMediaFileId", $"The file {mediaFileId} is not in the media library.",
                "Content.Home.Logo.NotFound", ValidationSeverity.Error);
    }

    /// <summary>Applies a full new order; the list must hold exactly the current IDs.</summary>
    public static bool TryReorder<T>(IReadOnlyList<T> items, IReadOnlyList<long> ids, Func<T, long> id,
        Action<T, int> moveTo)
    {
        if (items.Count != ids.Count || items.Any(item => !ids.Contains(id(item))))
            return false;
        var byId = items.ToDictionary(id);
        for (var position = 0; position < ids.Count; position++)
            moveTo(byId[ids[position]], position);
        return true;
    }
}

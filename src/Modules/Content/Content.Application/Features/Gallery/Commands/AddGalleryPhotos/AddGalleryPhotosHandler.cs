using Content.Application.Abstractions.Media;
using Content.Application.Features.Gallery.Shared;

namespace Content.Application.Features.Gallery.Commands.AddGalleryPhotos;

/// <summary>
/// Appends the chosen library pictures; a picture already in the gallery is skipped, a file that is not in the
/// library answers 400 and nothing is added.
/// </summary>
public sealed class AddGalleryPhotosHandler(
    IContentRepository<GalleryItem> repository,
    IReadContentDbContext db,
    IMediaImageReader images,
    TimeProvider clock)
    : ICommandHandler<AddGalleryPhotosCommand, Result<IReadOnlyList<AdminGalleryItem>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminGalleryItem>>> Handle(AddGalleryPhotosCommand command,
        CancellationToken cancellationToken)
    {
        var ids = command.MediaFileIds.Distinct().ToList();
        var files = await images.GetImagesAsync(ids, cancellationToken);
        var missing = ids.Where(id => !files.ContainsKey(id)).ToList();
        if (missing.Count > 0)
        {
            return Result.Invalid(new ValidationError("mediaFileIds",
                $"Цих файлів немає в медіатеці: {string.Join(", ", missing)}.",
                "Content.Gallery.MediaFile.NotFound", ValidationSeverity.Error));
        }

        var present = await db.GalleryItems.AsNoTracking()
            .Where(item => item.MediaFileId != null && ids.Contains(item.MediaFileId.Value))
            .Select(item => item.MediaFileId!.Value)
            .ToListAsync(cancellationToken);
        var now = clock.GetUtcNow();
        var position = await GalleryAdminList.NextPositionAsync(db, cancellationToken);
        var added = new List<GalleryItem>();
        foreach (var id in ids.Where(id => !present.Contains(id)))
        {
            var photo = GalleryItem.CreatePhoto(id, position++, GalleryItemTexts.Empty, now);
            if (photo.IsFailure)
                return photo.Error!.ToResult<IReadOnlyList<AdminGalleryItem>>();
            added.Add(photo.Value);
        }
        if (added.Count > 0)
            await repository.AddRangeAsync(added, cancellationToken);

        return Result.Success(await GalleryAdminList.LoadAsync(db, images, cancellationToken));
    }
}

using BuildingBlocks.Application.Contracts;
using Content.Application.Contracts.MediaLibrary;
using Content.Application.Features.Editing;
using Content.Application.Features.SeoSettings.Common;

namespace Content.Application.Features.SeoSettings.Update;

/// <summary>
/// Replaces all settings at once; the row comes from the migration, so a missing one is 404. A picture must still be
/// in the media library.
/// </summary>
public sealed class UpdateSiteSeoSettingsCommandHandler(
    IContentRepository<SiteSeoSettings> repository,
    IMediaImageReader images,
    ICurrentUserService currentUser,
    TimeProvider clock)
    : ICommandHandler<UpdateSiteSeoSettingsCommand, Result<SiteSeoSettingsDetails>>
{
    public async ValueTask<Result<SiteSeoSettingsDetails>> Handle(UpdateSiteSeoSettingsCommand command,
        CancellationToken cancellationToken)
    {
        var settings = await repository.GetByIdAsync(SiteSeoSettings.SingletonId, cancellationToken);
        if (settings is null)
            return Result.NotFound();

        if (command.Input.ShareImageMediaFileId is { } imageId
            && imageId != settings.ShareImageMediaFileId
            && !(await images.GetImagesAsync([imageId], cancellationToken)).ContainsKey(imageId))
        {
            return Result.Invalid(new ValidationError("shareImageMediaFileId",
                $"The file {imageId} is not in the media library.", "Content.SiteSeoSettings.Image.NotFound",
                ValidationSeverity.Error));
        }

        settings.Update(SiteSeoSettingsMapper.ToValues(command.Input), clock.GetUtcNow(),
            ContentEditor.Name(currentUser));
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(await SiteSeoSettingsMapper.ToDetailsAsync(settings, images, cancellationToken));
    }
}

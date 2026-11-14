using BuildingBlocks.Application.Contracts;
using Content.Application.Abstractions.Media;
using Content.Application.Shared;
using Content.Application.Features.SeoSettings.Shared;

namespace Content.Application.Features.SeoSettings.Commands.UpdateSiteSeoSettings;

/// <summary>
/// Replaces all settings at once; the row comes from the migration, so a missing one is 404. A picture must still be
/// in the media library.
/// </summary>
public sealed class UpdateSiteSeoSettingsHandler(
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
                "Цього зображення немає в медіатеці.", "Content.SiteSeoSettings.Image.NotFound",
                ValidationSeverity.Error));
        }

        var updated = settings.Update(SiteSeoSettingsMapper.ToValues(command.Input), clock.GetUtcNow(),
            ContentEditor.Name(currentUser));
        if (updated.IsFailure)
            return updated.Error!.ToResult<SiteSeoSettingsDetails>();
        await repository.SaveChangesAsync(cancellationToken);
        return Result.Success(await SiteSeoSettingsMapper.ToDetailsAsync(settings, images, cancellationToken));
    }
}

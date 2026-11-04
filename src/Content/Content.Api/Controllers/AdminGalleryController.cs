using Content.Api.Gallery;
using Content.Application.Features.Gallery.Admin.AddPhotos;
using Content.Application.Features.Gallery.Admin.AddVideo;
using Content.Application.Features.Gallery.Admin.Delete;
using Content.Application.Features.Gallery.Admin.GetList;
using Content.Application.Features.Gallery.Admin.PreviewYoutube;
using Content.Application.Features.Gallery.Admin.Reorder;
using Content.Application.Features.Gallery.Admin.Update;
using Content.Application.Features.Gallery.Common;
using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Controllers;

/// <summary>
/// Admin editing of the site gallery. Every change answers with the whole gallery in site order, so the page
/// needs no second request.
/// </summary>
[ApiController]
[Route("api/admin/content/gallery")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminGalleryController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminGallery")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminGalleryItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<AdminGalleryItemResponse>>> GetList(CancellationToken cancellationToken)
        => Map(await sender.Send(new GetAdminGalleryQuery(), cancellationToken));

    /// <summary>Adds media-library pictures to the end; a picture already in the gallery is skipped.</summary>
    [HttpPost("photos", Name = "addAdminGalleryPhotos")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminGalleryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<AdminGalleryItemResponse>>> AddPhotos(
        [FromBody] AddGalleryPhotosRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(new AddGalleryPhotosCommand(request.MediaFileIds ?? []), cancellationToken));

    /// <summary>Adds a YouTube video by its link to the end. 409 when the video is already there.</summary>
    [HttpPost("videos", Name = "addAdminGalleryVideo")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminGalleryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<AdminGalleryItemResponse>>> AddVideo(
        [FromBody] AddGalleryVideoRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(new AddGalleryVideoCommand(request.Url ?? string.Empty,
            new GalleryTextsInput(request.TitleUk, request.TitleRu, request.CaptionUk, request.CaptionRu)),
            cancellationToken));

    /// <summary>Video ID, poster and the title YouTube gives (<c>null</c> if it does not answer) for a pasted link.</summary>
    [HttpGet("youtube-preview", Name = "previewAdminGalleryYoutubeVideo")]
    [ProducesResponseType(typeof(YoutubePreviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<YoutubePreviewResponse>> PreviewYoutube(
        [FromQuery] string? url, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new PreviewYoutubeVideoQuery(url), cancellationToken);
        return this.ToActionResult(result.Map(GalleryResponseMapper.ToResponse));
    }

    [HttpPut("items/{id:long}", Name = "updateAdminGalleryItem")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminGalleryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AdminGalleryItemResponse>>> Update(
        [FromRoute] long id, [FromBody] UpdateGalleryItemRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(new UpdateGalleryItemCommand(id,
            new GalleryTextsInput(request.TitleUk, request.TitleRu, request.CaptionUk, request.CaptionRu)),
            cancellationToken));

    /// <summary>Removes a card; the picture stays in the media library.</summary>
    [HttpDelete("items/{id:long}", Name = "deleteAdminGalleryItem")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminGalleryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<AdminGalleryItemResponse>>> Delete(
        [FromRoute] long id, CancellationToken cancellationToken)
        => Map(await sender.Send(new DeleteGalleryItemCommand(id), cancellationToken));

    /// <summary>Saves the order of all cards. 409 when the list does not match the current gallery.</summary>
    [HttpPut("order", Name = "reorderAdminGallery")]
    [ProducesResponseType(typeof(IReadOnlyList<AdminGalleryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IReadOnlyList<AdminGalleryItemResponse>>> Reorder(
        [FromBody] ReorderGalleryRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(new ReorderGalleryCommand(request.Ids ?? []), cancellationToken));

    private ActionResult<IReadOnlyList<AdminGalleryItemResponse>> Map(Result<IReadOnlyList<AdminGalleryItem>> result)
        => this.ToActionResult(result.Map(GalleryResponseMapper.ToResponse));
}

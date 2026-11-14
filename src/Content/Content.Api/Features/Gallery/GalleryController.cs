using Content.Application.Features.Gallery.Queries.GetPublicGallery;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.Gallery;

/// <summary>The public site gallery: photos from the media library and YouTube videos, in site order.</summary>
[ApiController]
[Route("api/content")]
[AllowAnonymous]
public sealed class GalleryController(ISender sender) : ControllerBase
{
    [HttpGet("{lang}/gallery", Name = "getPublicGallery")]
    [ProducesResponseType(typeof(IReadOnlyList<PublicGalleryItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PublicGalleryItemResponse>>> GetList(
        [FromRoute] string lang, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicGalleryQuery(lang), cancellationToken);
        return this.ToActionResult(result.Map(items =>
            (IReadOnlyList<PublicGalleryItemResponse>)items.Select(GalleryResponseMapper.ToResponse).ToList()));
    }
}

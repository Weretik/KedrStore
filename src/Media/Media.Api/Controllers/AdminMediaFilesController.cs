using Identity.Contracts.Authorization;
using Media.Api.Requests;
using Media.Application.Contracts.Images;
using Media.Application.Features.Files.Common;
using Media.Application.Features.Files.Delete;
using Media.Application.Features.Files.GetById;
using Media.Application.Features.Files.GetList;
using Media.Application.Features.Files.Move;
using Media.Application.Features.Files.Replace;
using Media.Application.Features.Files.Update;
using Media.Application.Features.Files.Upload;
using Microsoft.AspNetCore.Authorization;

namespace Media.Api.Controllers;

/// <summary>Pictures of the media library on the CDN. Delete is for Admins only.</summary>
[ApiController]
[Route("api/admin/media/files")]
[Authorize(Policy = PolicyNames.CanManageMedia)]
public sealed class AdminMediaFilesController(ISender sender) : ControllerBase
{
    private const long UploadRequestLimit = MediaUploadRules.MaxUploadBytes + 64 * 1024;

    [HttpGet(Name = "getAdminMediaFiles")]
    [ProducesResponseType(typeof(MediaFilePage), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MediaFilePage>> GetList(
        [FromQuery] long? folderId, [FromQuery] bool inRoot = false, [FromQuery] string? search = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 60, [FromQuery] bool includeSubfolders = false,
        CancellationToken cancellationToken = default)
        => this.ToActionResult(await sender.Send(
            new GetMediaFilesQuery(folderId, inRoot, search, page, pageSize, includeSubfolders), cancellationToken));

    [HttpGet("{id:long}", Name = "getAdminMediaFile")]
    [ProducesResponseType(typeof(MediaFileItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaFileItem>> GetById([FromRoute] long id, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetMediaFileQuery(id), cancellationToken));

    [HttpPost(Name = "uploadAdminMediaFile")]
    [RequestSizeLimit(UploadRequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = UploadRequestLimit)]
    [ProducesResponseType(typeof(MediaFileItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<MediaFileItem>> Upload(
        IFormFile file, [FromForm] long? folderId, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UploadMediaFileCommand(folderId, ToUpload(file)), cancellationToken));

    [HttpPut("{id:long}", Name = "updateAdminMediaFile")]
    [ProducesResponseType(typeof(MediaFileItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MediaFileItem>> Update(
        [FromRoute] long id, [FromBody] UpdateMediaFileRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(
            new UpdateMediaFileCommand(id, request.FileName, request.FolderId), cancellationToken));

    [HttpPut("{id:long}/content", Name = "replaceAdminMediaFile")]
    [RequestSizeLimit(UploadRequestLimit)]
    [RequestFormLimits(MultipartBodyLengthLimit = UploadRequestLimit)]
    [ProducesResponseType(typeof(MediaFileItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<MediaFileItem>> Replace(
        [FromRoute] long id, IFormFile file, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new ReplaceMediaFileCommand(id, ToUpload(file)), cancellationToken));

    [HttpPost("move", Name = "moveAdminMediaFiles")]
    [ProducesResponseType(typeof(MoveMediaFilesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MoveMediaFilesResponse>> Move(
        [FromBody] MoveMediaFilesRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new MoveMediaFilesCommand(request.FileIds ?? [], request.FolderId), cancellationToken);
        return this.ToActionResult(result.Map(moved => new MoveMediaFilesResponse(moved)));
    }

    [HttpDelete("{id:long}", Name = "deleteAdminMediaFile")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteMediaFileCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    /// <summary>A missing file becomes <c>null</c>, which the validator answers with 400.</summary>
    private static MediaUpload ToUpload(IFormFile? file)
        => file is null ? null! : new(file.FileName, file.ContentType, file.Length, file.OpenReadStream);
}

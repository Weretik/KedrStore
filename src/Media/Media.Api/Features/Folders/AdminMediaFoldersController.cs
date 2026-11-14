using Identity.Contracts.Authorization;
using Media.Application.Features.Folders.Shared;
using Media.Application.Features.Folders.Commands.CreateMediaFolder;
using Media.Application.Features.Folders.Commands.DeleteMediaFolder;
using Media.Application.Features.Folders.Queries.GetMediaFolderTree;
using Media.Application.Features.Folders.Commands.UpdateMediaFolder;
using Microsoft.AspNetCore.Authorization;

namespace Media.Api.Features.Folders;

/// <summary>Folders of the media library. Delete is for Admins only.</summary>
[ApiController]
[Route("api/admin/media/folders")]
[Authorize(Policy = PolicyNames.CanManageMedia)]
public sealed class AdminMediaFoldersController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminMediaFolders")]
    [ProducesResponseType(typeof(MediaFolderTree), StatusCodes.Status200OK)]
    public async Task<ActionResult<MediaFolderTree>> GetTree(CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new GetMediaFolderTreeQuery(), cancellationToken));

    [HttpPost(Name = "createAdminMediaFolder")]
    [ProducesResponseType(typeof(MediaFolderItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MediaFolderItem>> Create(
        [FromBody] SaveMediaFolderRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new CreateMediaFolderCommand(request.Name, request.ParentId), cancellationToken));

    [HttpPut("{id:long}", Name = "updateAdminMediaFolder")]
    [ProducesResponseType(typeof(MediaFolderItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MediaFolderItem>> Update(
        [FromRoute] long id, [FromBody] SaveMediaFolderRequest request, CancellationToken cancellationToken)
        => this.ToActionResult(await sender.Send(new UpdateMediaFolderCommand(id, request.Name, request.ParentId), cancellationToken));

    [HttpDelete("{id:long}", Name = "deleteAdminMediaFolder")]
    [Authorize(Policy = PolicyNames.RequireAdminRole)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteMediaFolderCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }
}

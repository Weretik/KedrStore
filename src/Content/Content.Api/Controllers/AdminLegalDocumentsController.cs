using Content.Api.Documents;
using Content.Application.Features.Documents.Admin.CreateDraft;
using Content.Application.Features.Documents.Admin.DeleteDraft;
using Content.Application.Features.Documents.Admin.GetList;
using Content.Application.Features.Documents.Admin.GetVersion;
using Content.Application.Features.Documents.Admin.GetVersions;
using Content.Application.Features.Documents.Admin.Publish;
using Content.Application.Features.Documents.Admin.UpdateDraft;
using Content.Application.Features.Documents.Common;
using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Controllers;

/// <summary>Admin editing of site documents: one draft per document, publish with «Редакція від», read-only history.</summary>
[ApiController]
[Route("api/admin/content/documents")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminLegalDocumentsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminLegalDocuments")]
    [ProducesResponseType(typeof(IReadOnlyList<LegalDocumentOverviewResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<LegalDocumentOverviewResponse>>> GetList(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLegalDocumentsQuery(), cancellationToken);
        return this.ToActionResult(result.Map(items =>
            (IReadOnlyList<LegalDocumentOverviewResponse>)items.Select(LegalDocumentResponseMapper.ToResponse).ToList()));
    }

    [HttpGet("{kind}/versions", Name = "getAdminLegalDocumentVersions")]
    [ProducesResponseType(typeof(IReadOnlyList<LegalDocumentVersionSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<LegalDocumentVersionSummaryResponse>>> GetVersions(
        [FromRoute] string kind, CancellationToken cancellationToken)
    {
        if (!LegalDocumentKinds.TryParse(kind, out var parsed))
            return NotFound();
        var result = await sender.Send(new GetLegalDocumentVersionsQuery(parsed), cancellationToken);
        return this.ToActionResult(result.Map(items =>
            (IReadOnlyList<LegalDocumentVersionSummaryResponse>)items.Select(LegalDocumentResponseMapper.ToResponse).ToList()));
    }

    /// <summary>Starts a draft; without a body it copies the published texts. 409 when a draft already exists.</summary>
    [HttpPost("{kind}/versions", Name = "createAdminLegalDocumentDraft")]
    [ProducesResponseType(typeof(LegalDocumentVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LegalDocumentVersionResponse>> CreateDraft(
        [FromRoute] string kind, [FromBody] SaveLegalDocumentDraftRequest? request, CancellationToken cancellationToken)
    {
        if (!LegalDocumentKinds.TryParse(kind, out var parsed))
            return NotFound();
        return Map(await sender.Send(
            new CreateLegalDocumentDraftCommand(parsed, LegalDocumentResponseMapper.ToInput(request)), cancellationToken));
    }

    [HttpGet("versions/{id:long}", Name = "getAdminLegalDocumentVersion")]
    [ProducesResponseType(typeof(LegalDocumentVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LegalDocumentVersionResponse>> GetVersion(
        [FromRoute] long id, CancellationToken cancellationToken)
        => Map(await sender.Send(new GetLegalDocumentVersionQuery(id), cancellationToken));

    /// <summary>Replaces both texts of a draft. 409 for a published or archived version.</summary>
    [HttpPut("versions/{id:long}", Name = "updateAdminLegalDocumentDraft")]
    [ProducesResponseType(typeof(LegalDocumentVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LegalDocumentVersionResponse>> UpdateDraft(
        [FromRoute] long id, [FromBody] SaveLegalDocumentDraftRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(
            new UpdateLegalDocumentDraftCommand(id, LegalDocumentResponseMapper.ToInput(request) ?? []), cancellationToken));

    [HttpDelete("versions/{id:long}", Name = "deleteAdminLegalDocumentDraft")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteDraft([FromRoute] long id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteLegalDocumentDraftCommand(id), cancellationToken);
        return result.IsSuccess ? NoContent() : this.ToActionResult(result);
    }

    /// <summary>Puts the draft on the site as the next number; the previous edition becomes read-only history.</summary>
    [HttpPost("versions/{id:long}/publish", Name = "publishAdminLegalDocumentDraft")]
    [ProducesResponseType(typeof(LegalDocumentVersionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<LegalDocumentVersionResponse>> Publish(
        [FromRoute] long id, [FromBody] PublishLegalDocumentRequest request, CancellationToken cancellationToken)
        => Map(await sender.Send(new PublishLegalDocumentDraftCommand(id, request.EffectiveFrom), cancellationToken));

    private ActionResult<LegalDocumentVersionResponse> Map(Result<LegalDocumentVersionDetails> result)
        => this.ToActionResult(result.Map(LegalDocumentResponseMapper.ToResponse));
}

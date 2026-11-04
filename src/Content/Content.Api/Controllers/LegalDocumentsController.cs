using Content.Api.Documents;
using Content.Application.Features.Documents.Common;
using Content.Application.Features.Documents.Public.GetPublished;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Controllers;

/// <summary>Public site documents: only the published edition is shown.</summary>
[ApiController]
[Route("api/content")]
[AllowAnonymous]
public sealed class LegalDocumentsController(ISender sender) : ControllerBase
{
    /// <summary><c>kind</c>: <c>public-offer</c>, <c>privacy-policy</c>, <c>returns-policy</c> or <c>delivery-payment</c>.</summary>
    [HttpGet("{lang}/documents/{kind}", Name = "getPublishedLegalDocument")]
    [ProducesResponseType(typeof(PublishedLegalDocumentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublishedLegalDocumentResponse>> GetPublished(
        [FromRoute] string lang, [FromRoute] string kind, CancellationToken cancellationToken)
    {
        if (!LegalDocumentKinds.TryParse(kind, out var parsed))
            return NotFound();
        var result = await sender.Send(new GetPublishedLegalDocumentQuery(parsed, lang), cancellationToken);
        return this.ToActionResult(result.Map(LegalDocumentResponseMapper.ToResponse));
    }
}

using Content.Application.Features.Documents.Commands.PublishLegalDocumentDraft;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Documents;

internal static class PublishAdminLegalDocumentDraftEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("versions/{id:long}/publish", Handle)
            .WithName("publishAdminLegalDocumentDraft")
            .Produces<LegalDocumentVersionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    /// <summary>Puts the draft on the site as the next number; the previous edition becomes read-only history.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] PublishLegalDocumentRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new PublishLegalDocumentDraftCommand(id, request.EffectiveFrom), cancellationToken))
            .Map(LegalDocumentResponseMapper.ToResponse)
            .ToHttpResult(http);
}

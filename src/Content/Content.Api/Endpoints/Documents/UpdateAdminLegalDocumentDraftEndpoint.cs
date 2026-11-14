using Content.Application.Features.Documents.Commands.UpdateLegalDocumentDraft;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Documents;

internal static class UpdateAdminLegalDocumentDraftEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("versions/{id:long}", Handle)
            .WithName("updateAdminLegalDocumentDraft")
            .Produces<LegalDocumentVersionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    /// <summary>Replaces both texts of a draft. 409 for a published or archived version.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] long id, [FromBody] SaveLegalDocumentDraftRequest request, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
        => (await sender.Send(
            new UpdateLegalDocumentDraftCommand(id, LegalDocumentResponseMapper.ToInput(request) ?? []), cancellationToken))
            .Map(LegalDocumentResponseMapper.ToResponse)
            .ToHttpResult(http);
}

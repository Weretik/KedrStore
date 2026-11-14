using Content.Application.Features.Documents.Commands.DeleteLegalDocumentDraft;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Content.Api.Endpoints.Documents.DeleteAdminLegalDocumentDraft;

internal static class DeleteAdminLegalDocumentDraftEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("versions/{id:long}", Handle)
            .WithName("deleteAdminLegalDocumentDraft")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    private static async Task<IResult> Handle(
        [FromRoute] long id, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteLegalDocumentDraftCommand(id), cancellationToken);
        return result.IsSuccess ? TypedResults.NoContent() : result.ToHttpResult(http);
    }
}

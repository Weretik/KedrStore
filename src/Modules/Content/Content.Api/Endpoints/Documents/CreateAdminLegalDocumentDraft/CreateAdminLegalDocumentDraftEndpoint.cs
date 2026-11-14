using Content.Application.Features.Documents.Commands.CreateLegalDocumentDraft;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Documents.Shared;

namespace Content.Api.Endpoints.Documents.CreateAdminLegalDocumentDraft;

internal static class CreateAdminLegalDocumentDraftEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{kind}/versions", Handle)
            .WithName("createAdminLegalDocumentDraft")
            .Produces<LegalDocumentVersionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

    /// <summary>Starts a draft; without a body it copies the published texts. 409 when a draft already exists.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] string kind, [FromBody] SaveLegalDocumentDraftRequest? request, ISender sender,
        HttpContext http, CancellationToken cancellationToken)
    {
        return (await sender.Send(
            new CreateLegalDocumentDraftCommand(kind, LegalDocumentResponseMapper.ToInput(request)), cancellationToken))
            .Map(LegalDocumentResponseMapper.ToResponse)
            .ToHttpResult(http);
    }
}

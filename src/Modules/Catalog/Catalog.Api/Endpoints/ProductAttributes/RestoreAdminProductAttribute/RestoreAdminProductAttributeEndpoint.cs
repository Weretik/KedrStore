using Catalog.Application.Features.ProductAttributes.Commands.RestoreProductAttribute;
using Catalog.Contracts.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.ProductAttributes.RestoreAdminProductAttribute;

internal static class RestoreAdminProductAttributeEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapDelete("attributes/{attributeId:int}/manual", Handle)
            .WithName("restoreAdminProductAttribute")
            .WithSummary("«Повернути автоматичне»")
            .WithDescription("Drops the manual values of the attribute and brings back the automatic ones kept for them (`hasAutomatic`). Positions automation never had disappear; new automatic positions come with the next recognition run.")
            .Produces<AdminProductAttributesDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    /// <summary>«Повернути автоматичне»: drops the manual values of one attribute.</summary>
    private static async Task<IResult> Handle(
        [FromRoute] int productId, [FromRoute] int attributeId, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new RestoreProductAttributeCommand(productId, attributeId),
            cancellationToken)).ToHttpResult(http);
}

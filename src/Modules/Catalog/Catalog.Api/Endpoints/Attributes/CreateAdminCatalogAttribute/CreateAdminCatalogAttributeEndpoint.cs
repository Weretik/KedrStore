using Catalog.Application.Features.Attributes.Commands.CreateCatalogAttribute;
using Catalog.Contracts.Attributes;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Attributes.CreateAdminCatalogAttribute;

internal static class CreateAdminCatalogAttributeEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("createAdminCatalogAttribute")
            .WithSummary("Add an attribute («Нова ознака», Admin)")
            .WithDescription("The attribute is manual from the start, so the dictionary import leaves it. Code and slug come from the request or from the name (a slug made from the name gets a number when taken); a taken code or explicit slug is rejected.")
            .RequireAuthorization(PolicyNames.CanManageSiteCatalog)
            .Produces<AdminCatalogAttributeDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    /// <summary>«Нова ознака»: an attribute of the owner's own (Admin and Seo only).</summary>
    private static async Task<IResult> Handle(
        [FromBody] CreateCatalogAttributeRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new CreateCatalogAttributeCommand(request), cancellationToken)).ToHttpResult(http);
}

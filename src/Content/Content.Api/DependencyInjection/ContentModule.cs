using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using Content.Api.Endpoints.Articles;
using Content.Api.Endpoints.Contacts;
using Content.Api.Endpoints.Documents;
using Content.Api.Endpoints.Gallery;
using Content.Api.Endpoints.Home;
using Content.Api.Endpoints.Overview;
using Content.Api.Endpoints.Requisites;
using Content.Api.Endpoints.SeoSettings;
using Content.Api.Endpoints.Site;
using Content.Api.Endpoints.SiteSeo;
using Content.Api.Endpoints.Wholesale;
using Content.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.DependencyInjection;

/// <summary>Registers the Content module: Application validators, Infrastructure, Contracts implementations, and endpoints.</summary>
public static class ContentModule
{
    public static IServiceCollection AddContentModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddContentInfrastructure(configuration);
        services.AddValidatorsFromAssembly(typeof(Content.Application.ContentApplicationAssemblyMarker).Assembly, includeInternalTypes: true);

        return services;
    }

    public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapArticlesEndpoints();
        endpoints.MapContactsEndpoints();
        endpoints.MapDocumentsEndpoints();
        endpoints.MapGalleryEndpoints();
        endpoints.MapHomeEndpoints();
        endpoints.MapOverviewEndpoints();
        endpoints.MapRequisitesEndpoints();
        endpoints.MapSeoSettingsEndpoints();
        endpoints.MapSiteEndpoints();
        endpoints.MapSiteSeoEndpoints();
        endpoints.MapWholesaleEndpoints();
        return endpoints;
    }
}

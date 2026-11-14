using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Content.Api.Endpoints.Articles.DeleteAdminArticle;

using Content.Api.Endpoints.Articles.GetArticleCover;
using Content.Api.Endpoints.Articles.GetAdminArticles;
using Content.Api.Endpoints.Articles.GetAdminArticle;
using Content.Api.Endpoints.Articles.UploadAdminArticleCover;
using Content.Api.Endpoints.Articles.GetArticles;
using Content.Api.Endpoints.Articles.GetArticle;
using Content.Api.Endpoints.Articles.MoveLegacyAdminArticleCovers;
using Content.Api.Endpoints.Articles.UpdateAdminArticle;
using Content.Api.Endpoints.Articles.CreateAdminArticle;

namespace Content.Api.Endpoints.Articles;

/// <summary>
/// Admin management of site articles, drafts included. Public site articles: only published ones are listed or
/// opened.
/// </summary>
public static class ArticlesEndpoints
{
    public static IEndpointRouteBuilder MapArticlesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminArticles = endpoints.MapGroup("api/admin/articles")
            .WithTags("AdminArticles")
            .RequireAuthorization(PolicyNames.CanManageContent);
        GetAdminArticlesEndpoint.Map(adminArticles);
        GetAdminArticleEndpoint.Map(adminArticles);
        CreateAdminArticleEndpoint.Map(adminArticles);
        UpdateAdminArticleEndpoint.Map(adminArticles);
        DeleteAdminArticleEndpoint.Map(adminArticles);
        UploadAdminArticleCoverEndpoint.Map(adminArticles);
        MoveLegacyAdminArticleCoversEndpoint.Map(adminArticles);

        var articles = endpoints.MapGroup("api/content")
            .WithTags("Articles")
            .AllowAnonymous();
        GetArticlesEndpoint.Map(articles);
        GetArticleEndpoint.Map(articles);
        GetArticleCoverEndpoint.Map(articles);

        return endpoints;
    }
}

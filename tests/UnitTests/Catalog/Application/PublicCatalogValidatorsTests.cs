using Catalog.Application.Features.Categories.Queries.GetCategories;
using Catalog.Application.Features.Products.Queries.GetProductBySlug;
using Catalog.Application.Features.Products.Queries.GetPublicProductList;
using Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModel;
using Catalog.Contracts.Products.GetList;

namespace UnitTests.Catalog.Application;

/// <summary>The public catalog reads check the language and the slug on the server, whatever the site sent.</summary>
public sealed class PublicCatalogValidatorsTests
{
    [Theory]
    [InlineData("uk", true)]
    [InlineData("RU", true)]
    [InlineData("en", false)]
    public void Categories_AcceptOnlyUkrainianAndRussian(string lang, bool valid)
        => Assert.Equal(valid, new GetCategoriesValidator().Validate(new GetCategoriesQuery(new CategoryFilter(), lang)).IsValid);

    [Fact]
    public void ProductBySlug_RejectsAnEmptySlugAndAPriceTypeBelowOne()
    {
        var result = new GetProductBySlugValidator().Validate(
            new GetProductBySlugQuery(new GetProductBySlugRequest(" ", 0, "uk")));

        Assert.Equal(["Request.Slug", "Request.PriceTypeId"], result.Errors.Select(e => e.PropertyName));
    }

    [Fact]
    public void SiteModel_RejectsATooLongSlug()
        => Assert.False(new GetPublicSiteModelValidator().Validate(new GetPublicSiteModelQuery("uk", new string('a', 201))).IsValid);

    [Fact]
    public void PublicProductList_UsesTheSharedListRules()
    {
        var result = new GetPublicProductListValidator().Validate(
            new GetPublicProductListQuery(new GetProductsRequest { PageSize = 500, PriceFrom = 10, PriceTo = 5 }));

        Assert.Equal(["Request.PageSize", "Request"], result.Errors.Select(e => e.PropertyName));
        Assert.All(result.Errors, error => Assert.DoesNotContain("Request", error.ErrorMessage));
    }
}

using Catalog.Application.Features.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetList;
using Catalog.Application.Features.Products.GetAdminDetail;

namespace UnitTests.Catalog.Application;

public sealed class GetMissingProductPhotosTests
{
    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public void Validator_RejectsInvalidPaging(int page, int pageSize)
    {
        var validator = new GetMissingProductPhotosQueryValidator();

        var result = validator.Validate(new GetMissingProductPhotosQuery(
            new GetMissingProductPhotosRequest { Page = page, PageSize = pageSize }));

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validator_AcceptsPagingBoundaries()
    {
        var validator = new GetMissingProductPhotosQueryValidator();

        var first = validator.Validate(new GetMissingProductPhotosQuery(
            new GetMissingProductPhotosRequest { Page = 1, PageSize = 1 }));
        var maximum = validator.Validate(new GetMissingProductPhotosQuery(
            new GetMissingProductPhotosRequest { Page = 1, PageSize = 100 }));

        Assert.True(first.IsValid);
        Assert.True(maximum.IsValid);
    }

    [Fact]
    public void Validator_RejectsOversizedSearchAndUnknownSort()
    {
        var validator = new GetMissingProductPhotosQueryValidator();
        var result = validator.Validate(new GetMissingProductPhotosQuery(
            new GetMissingProductPhotosRequest
            {
                SearchTerm = new string('a', 101),
                Sort = (ProductSort)999
            }));

        Assert.False(result.IsValid);
        Assert.Equal(2, result.Errors.Count);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void DetailValidator_RejectsEmptySlug(string slug)
    {
        var result = new GetAdminProductBySlugQueryValidator()
            .Validate(new GetAdminProductBySlugQuery(slug));

        Assert.False(result.IsValid);
    }
}

using Catalog.Application.Features.Products.GetMissingPhotos;
using Catalog.Contracts.Products.GetMissingPhotos;

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
}

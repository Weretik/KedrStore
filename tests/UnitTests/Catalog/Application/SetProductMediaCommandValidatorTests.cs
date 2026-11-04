using Catalog.Application.Features.ProductMedia.Set;
using Catalog.Contracts.Products.Media;

namespace UnitTests.Catalog.Application;

/// <summary>R-30, R-31 input rules for <c>PUT /api/admin/products/{id}/media</c>.</summary>
public sealed class SetProductMediaCommandValidatorTests
{
    private readonly SetProductMediaCommandValidator _validator = new();

    [Fact]
    public void AcceptsOrderedPhotosAndScheme_OrAnEmptySet()
    {
        Assert.True(Validate([2, 1], 3).IsValid);
        Assert.True(Validate([], null).IsValid);
    }

    [Theory]
    [InlineData(new long[] { 1, 1 }, null, "photoIds")]
    [InlineData(new long[] { 0 }, null, "photoIds")]
    [InlineData(new long[] { 1 }, 1L, "schemeId")]
    [InlineData(new long[] { 1 }, -5L, "schemeId")]
    public void RejectsInvalidSets(long[] photoIds, long? schemeId, string property)
    {
        var result = Validate(photoIds, schemeId);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName.StartsWith(property, StringComparison.Ordinal));
    }

    [Fact]
    public void RejectsMoreThanTwentyPhotos()
        => Assert.False(Validate([.. Enumerable.Range(1, 21).Select(id => (long)id)], null).IsValid);

    private FluentValidation.Results.ValidationResult Validate(IReadOnlyList<long> photoIds, long? schemeId)
        => _validator.Validate(new SetProductMediaCommand(731,
            new SetProductMediaRequest { PhotoIds = photoIds, SchemeId = schemeId }));
}

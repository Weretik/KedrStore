using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;
using BuildingBlocks.Domain.Abstractions.Errors;

namespace UnitTests.BuildingBlocks.Results;

/// <summary>Asserts that a business rule reported an error, and returns it for further checks.</summary>
public static class DomainAssert
{
    public static DomainError Fails(DomainResult result)
    {
        Assert.True(result.IsFailure, "Expected the business rule to report an error.");
        return result.Error!;
    }

    public static DomainError Fails<T>(DomainResult<T> result)
    {
        Assert.True(result.IsFailure, "Expected the business rule to report an error.");
        return result.Error!;
    }

    public static void Succeeds(DomainResult result)
        => Assert.True(result.IsSuccess, result.Error is { } error ? $"{error.Code}: {error.Message}" : null);

    public static T Succeeds<T>(DomainResult<T> result)
    {
        Assert.True(result.IsSuccess, result.Error is { } error ? $"{error.Code}: {error.Message}" : null);
        return result.Value;
    }
}

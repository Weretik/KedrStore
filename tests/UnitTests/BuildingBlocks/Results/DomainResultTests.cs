using Ardalis.Result;
using BuildingBlocks.Application.Results;
using BuildingBlocks.Domain.Abstractions;
using BuildingBlocks.Domain.Results;

namespace UnitTests.BuildingBlocks.Results;

public sealed class DomainResultTests
{
    private static readonly DomainError Rule = new("Test.Rule", "Заповніть це поле.", Field: "Name");

    [Fact]
    public void AValueOrAnError_ConvertsImplicitly()
    {
        DomainResult<int> success = 5;
        DomainResult<int> failure = Rule;

        Assert.Equal(5, success.Value);
        Assert.Same(Rule, failure.Error);
        Assert.Throws<InvalidOperationException>(() => failure.Value);
    }

    [Fact]
    public void Combine_ReturnsTheFirstFailure()
    {
        var other = DomainError.Conflict("Test.Other", "Інша помилка.");

        Assert.Same(Rule, DomainResult.Combine(DomainResult.Success(), Rule, other).Error);
        Assert.True(DomainResult.Combine(DomainResult.Success()).IsSuccess);
    }

    [Theory]
    [InlineData(ErrorType.Validation, ResultStatus.Invalid)]
    [InlineData(ErrorType.NotFound, ResultStatus.NotFound)]
    [InlineData(ErrorType.Conflict, ResultStatus.Conflict)]
    [InlineData(ErrorType.Forbidden, ResultStatus.Forbidden)]
    [InlineData(ErrorType.Unexpected, ResultStatus.CriticalError)]
    public void TheErrorType_PicksTheResultStatus(ErrorType type, ResultStatus status)
        => Assert.Equal(status, (Rule with { Type = type }).ToResult<string>().Status);

    [Fact]
    public void AValidationError_NamesItsField_AndKeepsTheCode()
    {
        var error = Assert.Single(Rule.ToResult().ValidationErrors);

        Assert.Equal("Name", error.Identifier);
        Assert.Equal("Test.Rule", error.ErrorCode);
        Assert.Equal("Заповніть це поле.", error.ErrorMessage);
    }
}

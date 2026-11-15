using Ardalis.Result;
using BuildingBlocks.Application.Behaviors;
using BuildingBlocks.Application.Results;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace UnitTests.BuildingBlocks.Errors;

public sealed class ExceptionBehaviorTests
{
    [Fact]
    public async Task ConcurrencyException_BecomesAConflict()
    {
        var result = await RunThrowing(new DbUpdateConcurrencyException("row changed"));

        Assert.Equal(ResultStatus.Conflict, result.Status);
        Assert.Equal(RowVersions.ChangedMessage, Assert.Single(result.Errors));
    }

    [Fact]
    public async Task OtherException_BecomesAnUnknownError()
    {
        var result = await RunThrowing(new InvalidOperationException("boom"));

        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Equal("Виникла невідома помилка.", Assert.Single(result.Errors));
    }

    private static async Task<Result<int>> RunThrowing(Exception exception)
        => await new ExceptionBehavior<Save, Result<int>>(NullLogger<ExceptionBehavior<Save, Result<int>>>.Instance)
            .Handle(new Save(), (_, _) => throw exception, CancellationToken.None);

    public sealed record Save : ICommand<Result<int>>;
}

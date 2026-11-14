using Ardalis.Result;
using BuildingBlocks.Api.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace UnitTests.BuildingBlocks.Results;

/// <summary>
/// Platform spec 005, R-003/R-004: success keeps its status and body; every failure is
/// <c>application/problem+json</c> with <c>code</c>, <c>detail</c>, <c>traceId</c> and, for 400, <c>errors</c>.
/// </summary>
public sealed class ResultHttpTests
{
    private const string TraceId = "trace-1";

    [Fact]
    public void Ok_WithValue_Answers200WithTheValue()
    {
        var answer = Assert.IsType<Ok<object>>(Result.Success(42).ToHttpResult(Http()));

        Assert.Equal(42, answer.Value);
    }

    [Fact]
    public void Ok_WithoutValue_Answers200WithoutBody() =>
        Assert.IsType<Ok>(Result.Success().ToHttpResult(Http()));

    [Fact]
    public void Created_Answers201WithTheValue()
    {
        var answer = Assert.IsType<JsonHttpResult<object>>(Result.Created("new").ToHttpResult(Http()));

        Assert.Equal(StatusCodes.Status201Created, answer.StatusCode);
        Assert.Equal("new", answer.Value);
    }

    [Fact]
    public void NoContent_Answers204() =>
        Assert.IsType<NoContent>(Result.NoContent().ToHttpResult(Http()));

    [Fact]
    public void Invalid_WithOwnCode_AnswersValidationProblemWithThatCodeAndFieldErrors()
    {
        var result = Result.Invalid(
            new ValidationError("Name", "Вкажіть назву.", "name_required", ValidationSeverity.Error),
            new ValidationError("Name", "Назва задовга.", "MaximumLengthValidator", ValidationSeverity.Error));

        var problem = Problem<HttpValidationProblemDetails>(result.ToHttpResult(Http()), 400);

        Assert.Equal("name_required", problem.Extensions["code"]);
        Assert.Equal("Вкажіть назву.", problem.Detail);
        Assert.Equal(["Вкажіть назву.", "Назва задовга."], problem.Errors["Name"]);
    }

    [Fact]
    public void Invalid_WithFluentValidationBuiltInCode_AnswersCodeValidation()
    {
        var result = Result.Invalid(new ValidationError("Request.Password", "Пароль задовгий.", "MaximumLengthValidator",
            ValidationSeverity.Error));

        var problem = Problem<HttpValidationProblemDetails>(result.ToHttpResult(Http()), 400);

        Assert.Equal(ProblemCodes.Validation, problem.Extensions["code"]);
        Assert.Equal(["Пароль задовгий."], problem.Errors["Request.Password"]);
    }

    [Theory]
    [InlineData(ResultStatus.NotFound, 404, "not_found")]
    [InlineData(ResultStatus.Conflict, 409, "conflict")]
    [InlineData(ResultStatus.Forbidden, 403, "forbidden")]
    [InlineData(ResultStatus.Unauthorized, 401, "unauthorized")]
    [InlineData(ResultStatus.Unavailable, 503, "unavailable")]
    public void Failure_AnswersProblemWithStatusCodeWord(ResultStatus status, int httpStatus, string code)
    {
        var problem = Problem<ProblemDetails>(Failed(status, "Повідомлення.").ToHttpResult(Http()), httpStatus);

        Assert.Equal(code, problem.Extensions["code"]);
        Assert.Equal("Повідомлення.", problem.Detail);
    }

    [Fact]
    public void Error_Answers500Unexpected()
    {
        var problem = Problem<ProblemDetails>(Result.Error("Виникла невідома помилка.").ToHttpResult(Http()), 500);

        Assert.Equal(ProblemCodes.Unexpected, problem.Extensions["code"]);
        Assert.Equal("Виникла невідома помилка.", problem.Detail);
    }

    [Fact]
    public void NotFound_WithoutMessage_KeepsTheUkrainianTitle()
    {
        var problem = Problem<ProblemDetails>(Result<int>.NotFound().ToHttpResult(Http()), 404);

        Assert.Null(problem.Detail);
        Assert.Equal("Не знайдено.", problem.Title);
    }

    private static Result Failed(ResultStatus status, string message) => status switch
    {
        ResultStatus.NotFound => Result.NotFound(message),
        ResultStatus.Conflict => Result.Conflict(message),
        ResultStatus.Forbidden => Result.Forbidden(message),
        ResultStatus.Unauthorized => Result.Unauthorized(message),
        ResultStatus.Unavailable => Result.Unavailable(message),
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };

    private static T Problem<T>(IResult answer, int status) where T : ProblemDetails
    {
        var problemResult = Assert.IsType<ProblemHttpResult>(answer);
        Assert.Equal(status, problemResult.StatusCode);
        Assert.Equal("application/problem+json", problemResult.ContentType);
        var problem = Assert.IsType<T>(problemResult.ProblemDetails);
        Assert.Equal(status, problem.Status);
        Assert.Equal(TraceId, problem.Extensions["traceId"]);
        return problem;
    }

    private static DefaultHttpContext Http() => new() { TraceIdentifier = TraceId };
}

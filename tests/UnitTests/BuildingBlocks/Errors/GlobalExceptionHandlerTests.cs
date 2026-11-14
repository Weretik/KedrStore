using BuildingBlocks.Api.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace UnitTests.BuildingBlocks.Errors;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandle_LogsTheException_AndAnswers500WithTraceIdAndNoDetails()
    {
        var logger = new RecordingLogger();
        var problems = new RecordingProblemDetailsService();
        var handler = new GlobalExceptionHandler(problems, logger);
        var context = new DefaultHttpContext { TraceIdentifier = "trace-42" };
        var exception = new InvalidOperationException("connection string Host=secret");

        var handled = await handler.TryHandleAsync(context, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Same(exception, logger.Exception);
        Assert.Equal(LogLevel.Error, logger.Level);
        var problem = Assert.IsType<ProblemDetails>(problems.Context?.ProblemDetails);
        Assert.Equal(StatusCodes.Status500InternalServerError, problem.Status);
        Assert.Equal("trace-42", problem.Extensions["traceId"]);
        Assert.DoesNotContain("secret", $"{problem.Title} {problem.Detail}");
        Assert.Same(exception, problems.Context!.Exception);
    }

    private sealed class RecordingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? Context { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Context = context;
            return ValueTask.FromResult(true);
        }
    }

    private sealed class RecordingLogger : ILogger<GlobalExceptionHandler>
    {
        public Exception? Exception { get; private set; }
        public LogLevel? Level { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Level = logLevel;
            Exception = exception;
        }
    }
}

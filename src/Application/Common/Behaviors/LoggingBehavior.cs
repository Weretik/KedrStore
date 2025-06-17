namespace Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IUseCase
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUser.UserId;
        var timer = Stopwatch.StartNew();

        logger.LogInformation("➡️ {RequestName} started by User: {UserId} | Payload: {@Request}",
            requestName, userId, request);

        TResponse response;

        try
        {
            response = await next(cancellationToken);
        }
        catch (Exception ex)
        {
            timer.Stop();

            logger.LogError(ex, "❌ {RequestName} crashed [{Elapsed}ms] | User: {UserId}",
                requestName, timer.ElapsedMilliseconds, userId);

            throw;
        }

        timer.Stop();

        if (response is IApplicationResult result)
        {
            if (result.IsSuccess)
            {
                logger.LogInformation("✅ {RequestName} succeeded [{Elapsed}ms] | User: {UserId} | Response: {@Response}",
                    requestName, timer.ElapsedMilliseconds, userId, response);
            }
            else
            {
                logger.LogWarning("⚠️ {RequestName} failed [{Elapsed}ms] | User: {UserId} | Error: {Error}",
                    requestName, timer.ElapsedMilliseconds, userId, result.Error?.ToString() ?? "Unknown error");
            }
        }
        else
        {
            logger.LogInformation("✅ {RequestName} completed [{Elapsed}ms] | User: {UserId} | Response: {@Response}",
                requestName, timer.ElapsedMilliseconds, userId, response);
        }

        return response;
    }
}


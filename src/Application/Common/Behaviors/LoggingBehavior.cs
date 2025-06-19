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

        logger.LogInformation($"➡️ {requestName} started by User: {userId} | Payload: {request}");

        TResponse response;

        try
        {
            response = await next(cancellationToken);
        }
        catch (Exception ex)
        {
            timer.Stop();

            logger.LogError(ex, $"❌ {requestName} crashed [{timer.ElapsedMilliseconds}ms] | User: {userId}");

            throw;
        }

        timer.Stop();

        if (response is IApplicationResult result)
        {
            if (result.IsSuccess)
            {
                logger.LogInformation($"✅ {requestName} succeeded [{timer.ElapsedMilliseconds}ms] | " +
                                      $"User: {userId} | Response: {response}");
            }
            else
            {
                logger.LogWarning($"⚠️ {requestName} failed [{timer.ElapsedMilliseconds}ms] | User: {userId} | " +
                                  $"Error: {result.Error?.ToString() ?? "Unknown error"}");
            }
        }
        else
        {
            logger.LogInformation($"✅ {requestName} completed [{timer.ElapsedMilliseconds}ms] | " +
                                  $"User: {userId} | Response: {response}");
        }

        return response;
    }
}


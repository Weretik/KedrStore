namespace Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(
    ILoggingService loggingService,
    ICurrentUserService currentUser,
    IEnvironmentService environmentService)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IUseCase
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = currentUser.UserId;
        var timer = Stopwatch.StartNew();

        var safeRequest = GetSafeRequestForLogging(request);

        loggingService.LogRequestStarted(requestName, userId,
            environmentService.IsProduction() ? null : safeRequest);

        TResponse response;

        try
        {
            response = await next(cancellationToken);
        }
        catch (Exception ex)
        {
            timer.Stop();
            loggingService.LogRequestFailed(requestName, timer.ElapsedMilliseconds, userId, ex);
            throw;
        }

        timer.Stop();

        if (response is IApplicationResult result)
        {
            if (result.IsSuccess)
                loggingService.LogRequestSucceeded(requestName, timer.ElapsedMilliseconds, userId, response);
            else
                loggingService.LogBusinessRuleBroken(requestName, result.Error?.Code ?? "Unknown", result.Error?.Message ?? "Unknown error");
        }
        else
        {
            loggingService.LogRequestSucceeded(requestName, timer.ElapsedMilliseconds, userId, response);
        }

        return response;
    }

    private static object? GetSafeRequestForLogging(TRequest request)
    {
        if (request is ISafeLoggable safe)
            return safe.ToSafeLog();

        return IsSensitive(request) ? null : request;
    }

    private static bool IsSensitive(object request)
    {
        //TODO: Реализовать логику определения чувствительности запроса после реализации регистрации/авторизации
        return false; // пока ничего не считаем чувствительным
    }
}

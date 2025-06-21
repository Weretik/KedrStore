namespace Application.Common.Behaviors;

public sealed class ExceptionHandlingBehavior<TRequest, TResponse>(
    ILogger<ExceptionHandlingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (AppException ex)
        {
            var requestName = typeof(TRequest).Name;

            logger.LogWarning($"⚠️ AppException in {requestName}: {ex.Code} | {ex.Message}");

            return CreateFailureResult(ex.Code, ex.Message);
        }
        catch (BusinessRuleValidationException ex)
        {
            var requestName = typeof(TRequest).Name;

            logger.LogWarning($"⚠️ BusinessRule broken in {requestName}: {ex.Code} | {ex.Message}");

            return CreateFailureResult(ex.Code, ex.Message);
        }
    }

    private static TResponse CreateFailureResult(string code, string message)
    {
        var error = new AppError(code, message);

        if (typeof(TResponse).IsGenericType &&
            typeof(TResponse).GetGenericTypeDefinition() == typeof(AppResult<>))
        {
            var valueType = typeof(TResponse).GenericTypeArguments[0];
            var method = typeof(AppResult<>)
                .MakeGenericType(valueType)
                .GetMethod(nameof(AppResult<object>.Failure))!;

            return (TResponse)method.Invoke(null, [error])!;
        }

        if (typeof(TResponse) == typeof(AppResult))
        {
            return (TResponse)(object)AppResult.Failure(error);
        }

        Throw.Application(AppErrors.System.UnsupportedResponseType.WithDetails(typeof(TResponse).Name));
        return default!;
    }
}

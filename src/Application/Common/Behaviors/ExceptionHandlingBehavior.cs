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

            logger.LogWarning("⚠️ AppException in {RequestName}: {Code} | {Description}",
                requestName, ex.Code, ex.Description);

            // Обработка AppResult<T>
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(AppResult<>))
            {
                var valueType = typeof(TResponse).GenericTypeArguments[0];
                var method = typeof(AppResult<>)
                    .MakeGenericType(valueType)
                    .GetMethod(nameof(AppResult<object>.Failure))!;

                return (TResponse)method.Invoke(null, new object[] {
                    new AppError(ex.Code, ex.Description)
                })!;
            }

            // Обработка AppResult (не типизированный)
            if (typeof(TResponse) == typeof(AppResult))
            {
                return (TResponse)(object)AppResult.Failure(new AppError(ex.Code, ex.Description));
            }

            // Неизвестный тип
            throw new InvalidOperationException(
                $"❌ ExceptionHandlingBehavior does not support response type {typeof(TResponse).Name}");
        }
    }
}

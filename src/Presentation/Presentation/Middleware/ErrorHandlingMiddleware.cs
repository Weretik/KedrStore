namespace Presentation.Middleware;

public sealed class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            logger.LogWarning("⚠️ AppException: {Code} | {Description}", ex.Code, ex.Description);
            await WriteErrorResponseAsync(
                context,
                ex.Code,
                ex.Description,
                HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "🔥 Unhandled exception");
            var error = AppErrors.System.Unexpected
                .WithDetails($"{ex.Message} | {ex.StackTrace} | {ex.InnerException?.Message}");

            await WriteErrorResponseAsync(
                context,
                error.Code,
                error.Description,
                HttpStatusCode.InternalServerError
            );
        }
    }

    private static async Task WriteErrorResponseAsync(
        HttpContext context,
        string code, string message,
        HttpStatusCode statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var errorResponse = new
        {
            error = new
            {
                code,
                message
            }
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }
}

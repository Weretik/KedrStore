using Serilog.Context;

namespace Web.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : "Anonymous";

            var ip = context.Connection.RemoteIpAddress?.ToString();
            var requestId = Guid.NewGuid(); // Можно заменить на real CorrelationId, если появится

            using (LogContext.PushProperty("User", user))
            using (LogContext.PushProperty("ClientIP", ip))
            using (LogContext.PushProperty("RequestPath", context.Request.Path))
            using (LogContext.PushProperty("RequestId", requestId))
                try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                

                _logger.LogError(ex, "Unhandled exception occurred while processing request.");

                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("An unexpected error occurred.");
            }
        }
    }
}

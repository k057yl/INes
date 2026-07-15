using INest.Constants;
using INest.Exceptions;
using System.Net;
using System.Text.Json;
using static INest.Constants.LocalizationConstants;

namespace INest.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(
            RequestDelegate next,
            ILogger<ExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = SharedConstants.CONTENT_TYPE_JSON;

            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = SYSTEM.DEFAULT_ERROR;
            object? details = _env.IsDevelopment()
                ? ex.StackTrace
                : null;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            switch (ex)
            {
                case ValidationAppException validationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = SYSTEM.VALIDATION_FAILED;

                    details = validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => JsonNamingPolicy.CamelCase.ConvertName(g.Key),
                            g => g.Select(x => x.ErrorMessage).ToArray());

                    break;

                case AppException appException:
                    statusCode = appException.StatusCode;
                    message = appException.Message;
                    break;

                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = AUTH.ERRORS.TOKEN_MISSING;
                    break;
            }

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new
                {
                    error = message,
                    details
                }, options));
        }
    }
}
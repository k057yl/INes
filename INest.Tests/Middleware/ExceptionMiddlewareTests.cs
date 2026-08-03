using System.Text.Json;
using FluentValidation.Results;
using INest.Exceptions;
using INest.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace INest.Tests.Middleware
{
    public class ExceptionMiddlewareTests
    {
        private readonly ILogger<ExceptionMiddleware> _loggerMock = Substitute.For<ILogger<ExceptionMiddleware>>();
        private readonly IHostEnvironment _envMock = Substitute.For<IHostEnvironment>();

        public ExceptionMiddlewareTests()
        {
            _envMock.EnvironmentName.Returns("Production");
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturnAppExceptionStatusCodeAndMessage()
        {
            // Arrange
            RequestDelegate next = (ctx) => throw new AppException("AUTH.ERRORS.INVALID_CREDENTIALS", 401);
            var middleware = new ExceptionMiddleware(next, _loggerMock, _envMock);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.ShouldBe(401);
            context.Response.ContentType.ShouldStartWith("application/json");

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var json = await reader.ReadToEndAsync();

            using var doc = JsonDocument.Parse(json);
            doc.RootElement.GetProperty("error").GetString().ShouldBe("AUTH.ERRORS.INVALID_CREDENTIALS");
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturnBadRequest_WhenValidationAppExceptionThrown()
        {
            // Arrange
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Title", "Title is required")
            };

            RequestDelegate next = (ctx) => throw new ValidationAppException(failures);
            var middleware = new ExceptionMiddleware(next, _loggerMock, _envMock);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.ShouldBe(400);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(context.Response.Body);
            var json = await reader.ReadToEndAsync();

            using var doc = JsonDocument.Parse(json);
            doc.RootElement.GetProperty("details").GetProperty("title")[0].GetString().ShouldBe("Title is required");
        }

        [Fact]
        public async Task InvokeAsync_ShouldReturn500InternalServerError_OnUnhandledException()
        {
            // Arrange
            RequestDelegate next = (ctx) => throw new InvalidOperationException("Unexpected error");
            var middleware = new ExceptionMiddleware(next, _loggerMock, _envMock);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            await middleware.InvokeAsync(context);

            // Assert
            context.Response.StatusCode.ShouldBe(500);
        }
    }
}
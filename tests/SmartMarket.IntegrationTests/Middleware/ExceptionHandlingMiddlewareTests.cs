using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using SmartMarket.Api.Middleware;
using SmartMarket.Application.Common;

namespace SmartMarket.IntegrationTests.Middleware;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenNextThrows_ReturnsServerErrorApiErrorResponse()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("Test failure."),
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType);

        context.Response.Body.Position = 0;
        var body = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(
            context.Response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.NotNull(body);
        Assert.Equal(StatusCodes.Status500InternalServerError, body.StatusCode);
        Assert.Equal(ErrorCodes.ServerError, body.Code);
        Assert.Equal("An unexpected error occurred.", body.Message);
        Assert.Empty(body.Errors);
    }
}

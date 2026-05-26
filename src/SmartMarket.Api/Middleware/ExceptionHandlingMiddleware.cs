using SmartMarket.Application.Common;

namespace SmartMarket.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception.");
            await WriteErrorResponseAsync(context);
        }
    }

    private static Task WriteErrorResponseAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var body = new ApiErrorResponse(
            StatusCodes.Status500InternalServerError,
            ErrorCodes.ServerError,
            "An unexpected error occurred.",
            Array.Empty<string>());

        return context.Response.WriteAsJsonAsync(body);
    }
}

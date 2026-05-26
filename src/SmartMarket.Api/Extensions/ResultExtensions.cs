using Microsoft.AspNetCore.Mvc;
using SmartMarket.Application.Common;

namespace SmartMarket.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return ToErrorActionResult(result.ErrorCode!, result.Message!, result.Errors);
    }

    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new OkResult();
        }

        return ToErrorActionResult(result.ErrorCode!, result.Message!, result.Errors);
    }

    private static ObjectResult ToErrorActionResult(string code, string message, IReadOnlyList<string> errors)
    {
        var statusCode = MapStatusCode(code);
        var body = new ApiErrorResponse(statusCode, code, message, errors);
        return new ObjectResult(body) { StatusCode = statusCode };
    }

    private static int MapStatusCode(string code) => code switch
    {
        ErrorCodes.ValidationFailed => StatusCodes.Status400BadRequest,
        ErrorCodes.AuthInvalidCredentials => StatusCodes.Status401Unauthorized,
        ErrorCodes.AuthUnauthorized => StatusCodes.Status401Unauthorized,
        ErrorCodes.AuthForbidden => StatusCodes.Status403Forbidden,
        ErrorCodes.AuthEmailExists => StatusCodes.Status409Conflict,
        ErrorCodes.ProductNotFound => StatusCodes.Status404NotFound,
        ErrorCodes.OrderNotFound => StatusCodes.Status404NotFound,
        ErrorCodes.ProductSkuExists => StatusCodes.Status409Conflict,
        ErrorCodes.ProductInactive => StatusCodes.Status409Conflict,
        ErrorCodes.CheckoutEmptyCart => StatusCodes.Status409Conflict,
        ErrorCodes.CheckoutProductUnavailable => StatusCodes.Status409Conflict,
        ErrorCodes.CheckoutInsufficientStock => StatusCodes.Status409Conflict,
        ErrorCodes.OrderInvalidStatusTransition => StatusCodes.Status409Conflict,
        ErrorCodes.CartItemNotFound => StatusCodes.Status404NotFound,
        ErrorCodes.CartInvalidQuantity => StatusCodes.Status400BadRequest,
        _ when code.StartsWith("product.", StringComparison.Ordinal) => StatusCodes.Status400BadRequest,
        _ when code.StartsWith("cart.", StringComparison.Ordinal) => StatusCodes.Status400BadRequest,
        _ when code.StartsWith("checkout.", StringComparison.Ordinal) => StatusCodes.Status409Conflict,
        _ => StatusCodes.Status400BadRequest
    };
}

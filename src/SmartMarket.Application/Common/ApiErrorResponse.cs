namespace SmartMarket.Application.Common;

public sealed record ApiErrorResponse(
    int StatusCode,
    string Code,
    string Message,
    IReadOnlyList<string> Errors);

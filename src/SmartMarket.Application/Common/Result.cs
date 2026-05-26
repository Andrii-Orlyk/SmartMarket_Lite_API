namespace SmartMarket.Application.Common;

public class Result
{
    protected Result(bool isSuccess, string? errorCode, string? message, IReadOnlyList<string>? errors = null)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        Message = message;
        Errors = errors ?? Array.Empty<string>();
    }

    public bool IsSuccess { get; }

    public string? ErrorCode { get; }

    public string? Message { get; }

    public IReadOnlyList<string> Errors { get; }

    public static Result Success() => new(true, null, null);

    public static Result Failure(string errorCode, string message, IReadOnlyList<string>? errors = null) =>
        new(false, errorCode, message, errors);
}

public sealed class Result<T> : Result
{
    private Result(bool isSuccess, T? value, string? errorCode, string? message, IReadOnlyList<string>? errors = null)
        : base(isSuccess, errorCode, message, errors)
    {
        Value = value;
    }

    public T? Value { get; }

    public static Result<T> Success(T value) => new(true, value, null, null);

    public new static Result<T> Failure(string errorCode, string message, IReadOnlyList<string>? errors = null) =>
        new(false, default, errorCode, message, errors);
}

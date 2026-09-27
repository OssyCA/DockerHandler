using System.Diagnostics.CodeAnalysis;

namespace DockerController.Core.Results;

public readonly record struct Result<T>
{
    private Result(bool isSuccess, T? value, ResultError error, string? message)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Message = message;
    }

    public bool IsSuccess { get; }

    public T? Value { get; }

    public ResultError Error { get; }

    public string? Message { get; }

    public static Result<T> Success(T value) => new(true, value, ResultError.None, null);

    public static Result<T> NotFound(string message) => new(false, default, ResultError.NotFound, message);

    public static Result<T> Conflict(string message) => new(false, default, ResultError.Conflict, message);

    public static Result<T> Invalid(string message) => new(false, default, ResultError.Validation, message);

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = Value;
        return IsSuccess && value is not null;
    }

    public Result WithoutValue() => IsSuccess ? Result.Success() : Result.Failure(Error, Message);
}

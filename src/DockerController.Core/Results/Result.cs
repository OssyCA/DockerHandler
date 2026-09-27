namespace DockerController.Core.Results;

public readonly record struct Result
{
    private Result(bool isSuccess, ResultError error, string? message)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
    }

    public bool IsSuccess { get; }

    public ResultError Error { get; }

    public string? Message { get; }

    public static Result Success() => new(true, ResultError.None, null);

    public static Result NotFound(string message) => new(false, ResultError.NotFound, message);

    public static Result Conflict(string message) => new(false, ResultError.Conflict, message);

    public static Result Invalid(string message) => new(false, ResultError.Validation, message);

    internal static Result Failure(ResultError error, string? message) => new(false, error, message);
}

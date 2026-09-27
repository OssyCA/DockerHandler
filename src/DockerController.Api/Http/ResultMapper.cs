using DockerController.Core.Results;

namespace DockerController.Api.Http;

public static class ResultMapper
{
    public static IResult ToHttpResult(this Result result) =>
        result.IsSuccess ? TypedResults.NoContent() : Problem(result.Error, result.Message);

    public static IResult ToHttpResult<T>(this Result<T> result) =>
        result.TryGetValue(out var value)
            ? TypedResults.Ok(value)
            : Problem(result.Error, result.Message);

    private static IResult Problem(ResultError error, string? message)
    {
        var (statusCode, code) = error switch
        {
            ResultError.NotFound => (StatusCodes.Status404NotFound, ErrorCodes.NotFound),
            ResultError.Conflict => (StatusCodes.Status409Conflict, ErrorCodes.Conflict),
            ResultError.Validation => (StatusCodes.Status400BadRequest, ErrorCodes.ValidationFailed),
            _ => (StatusCodes.Status500InternalServerError, ErrorCodes.InternalError),
        };

        return TypedResults.Problem(
            detail: message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { [ErrorCodes.PropertyName] = code });
    }
}

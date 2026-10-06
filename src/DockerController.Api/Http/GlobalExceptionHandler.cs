using DockerController.Core.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DockerController.Api.Http;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, code, title) = Map(exception);

        logger.LogError(
            exception,
            "Obehandlat fel i {Method} {Path} gav {StatusCode} med kod {ErrorCode}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            statusCode,
            code);

        httpContext.Response.StatusCode = statusCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = environment.IsDevelopment() ? exception.Message : null,
                Extensions = { [ErrorCodes.PropertyName] = code },
            },
        });
    }

    private static (int StatusCode, string Code, string Title) Map(Exception exception) => exception switch
    {
        DockerDaemonUnavailableException => (
            StatusCodes.Status503ServiceUnavailable,
            ErrorCodes.DockerDaemonUnavailable,
            "Docker-daemonen är inte tillgänglig."),

        DockerSocketAccessDeniedException => (
            StatusCodes.Status503ServiceUnavailable,
            ErrorCodes.DockerSocketAccessDenied,
            "Controllern saknar behörighet till Docker-socketen."),

        DockerProtocolException => (
            StatusCodes.Status500InternalServerError,
            ErrorCodes.DockerProtocolError,
            "Oväntat svar från Docker-daemonen."),

        _ => (
            StatusCodes.Status500InternalServerError,
            ErrorCodes.InternalError,
            "Ett internt fel inträffade."),
    };
}

using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Docker.DotNet;
using DockerController.Core.Exceptions;

namespace DockerController.Docker;

public static class DockerExceptionTranslator
{
    public static Exception Translate(Exception exception, Uri endpoint, CancellationToken cancellationToken)
    {
        if (exception is AggregateException { InnerException: { } aggregated })
        {
            return Translate(aggregated, endpoint, cancellationToken);
        }

        if (exception is DockerContainerNotFoundException or DockerControllerException)
        {
            return exception;
        }

        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            return exception;
        }

        if (IsAccessDenied(exception))
        {
            return new DockerSocketAccessDeniedException($"Saknar behörighet till {endpoint}.", exception);
        }

        return exception switch
        {
            HttpRequestException or SocketException or IOException or TimeoutException or OperationCanceledException =>
                new DockerDaemonUnavailableException($"Når inte Docker-daemonen på {endpoint}.", exception),

            JsonException => new DockerProtocolException("Kunde inte tolka svaret från Docker-daemonen.", exception),

            DockerApiException { StatusCode: >= HttpStatusCode.InternalServerError } apiException =>
                new DockerDaemonUnavailableException(
                    $"Docker-daemonen svarade med {(int)apiException.StatusCode}.", exception),

            DockerApiException apiException => new DockerProtocolException(
                $"Oväntat svar från Docker-daemonen: {(int)apiException.StatusCode}.", exception),

            _ => exception,
        };
    }

    private static bool IsAccessDenied(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            if (current is UnauthorizedAccessException or SocketException { SocketErrorCode: SocketError.AccessDenied })
            {
                return true;
            }
        }

        return false;
    }
}

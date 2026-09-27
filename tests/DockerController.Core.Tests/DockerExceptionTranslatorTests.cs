using System.Net.Sockets;
using DockerController.Core.Exceptions;
using DockerController.Docker;

namespace DockerController.Core.Tests;

public class DockerExceptionTranslatorTests
{
    private static readonly Uri Endpoint = new("unix:///var/run/docker.sock");

    private static Exception Translate(Exception exception, CancellationToken cancellationToken = default) =>
        DockerExceptionTranslator.Translate(exception, Endpoint, cancellationToken);

    [Fact]
    public void Socket_failure_becomes_daemon_unavailable()
    {
        var translated = Translate(new SocketException((int)SocketError.ConnectionRefused));

        Assert.IsType<DockerDaemonUnavailableException>(translated);
    }

    [Fact]
    public void Timed_out_pipe_becomes_daemon_unavailable()
    {
        var translated = Translate(new TaskCanceledException("The operation was canceled."));

        Assert.IsType<DockerDaemonUnavailableException>(translated);
    }

    [Fact]
    public void Aggregate_exception_is_unwrapped_before_translation()
    {
        var translated = Translate(
            new AggregateException(new SocketException((int)SocketError.ConnectionRefused)));

        Assert.IsType<DockerDaemonUnavailableException>(translated);
    }

    [Fact]
    public void Access_denied_inside_http_request_exception_is_detected()
    {
        var translated = Translate(
            new HttpRequestException("Connection failed.", new SocketException((int)SocketError.AccessDenied)));

        Assert.IsType<DockerSocketAccessDeniedException>(translated);
    }

    [Fact]
    public void Unauthorized_access_becomes_socket_access_denied()
    {
        var translated = Translate(new UnauthorizedAccessException());

        Assert.IsType<DockerSocketAccessDeniedException>(translated);
    }

    [Fact]
    public void Cancellation_requested_by_the_caller_is_left_alone()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var original = new OperationCanceledException();
        var translated = Translate(original, cancellation.Token);

        Assert.Same(original, translated);
    }

    [Fact]
    public void Unknown_exceptions_are_left_alone()
    {
        var original = new InvalidOperationException("bugg i vår egen kod");

        Assert.Same(original, Translate(original));
    }
}

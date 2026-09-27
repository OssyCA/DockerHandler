namespace DockerController.Api.Http;

public static class ErrorCodes
{
    public const string PropertyName = "code";

    public const string NotFound = "not_found";
    public const string Conflict = "conflict";
    public const string ValidationFailed = "validation_failed";
    public const string DockerDaemonUnavailable = "docker_daemon_unavailable";
    public const string DockerSocketAccessDenied = "docker_socket_access_denied";
    public const string DockerProtocolError = "docker_protocol_error";
    public const string InternalError = "internal_error";
}

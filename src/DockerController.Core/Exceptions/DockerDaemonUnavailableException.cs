namespace DockerController.Core.Exceptions;

public sealed class DockerDaemonUnavailableException : DockerControllerException
{
    public DockerDaemonUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

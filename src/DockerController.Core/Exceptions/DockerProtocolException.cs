namespace DockerController.Core.Exceptions;

public sealed class DockerProtocolException : DockerControllerException
{
    public DockerProtocolException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

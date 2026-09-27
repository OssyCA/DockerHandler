namespace DockerController.Core.Exceptions;

public sealed class DockerSocketAccessDeniedException : DockerControllerException
{
    public DockerSocketAccessDeniedException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

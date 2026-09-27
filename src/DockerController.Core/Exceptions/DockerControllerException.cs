namespace DockerController.Core.Exceptions;

public abstract class DockerControllerException : Exception
{
    protected DockerControllerException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}

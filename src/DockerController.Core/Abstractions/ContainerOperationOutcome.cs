namespace DockerController.Core.Abstractions;

public enum ContainerOperationOutcome
{
    NotFound = 0,
    Changed,
    AlreadyInDesiredState,
}

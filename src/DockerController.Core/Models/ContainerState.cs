namespace DockerController.Core.Models;

public enum ContainerState
{
    Unknown = 0,
    Created,
    Running,
    Paused,
    Restarting,
    Removing,
    Exited,
    Dead,
}

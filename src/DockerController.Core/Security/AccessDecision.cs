namespace DockerController.Core.Security;

public enum AccessDecision
{
    DeniedNotManaged = 0,
    DeniedByName,
    Allowed,
}

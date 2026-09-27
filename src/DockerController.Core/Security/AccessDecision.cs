namespace DockerController.Core.Security;

public enum AccessDecision
{
    // 0 = nekad: ett oinitierat beslut får aldrig släppa igenom.
    DeniedNotManaged = 0,
    DeniedByName,
    Allowed,
}

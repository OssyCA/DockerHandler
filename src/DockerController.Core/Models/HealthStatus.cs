namespace DockerController.Core.Models;

public enum HealthStatus
{
    // 0 = Unhealthy: ett oinitierat tillstånd får inte rapporteras som friskt.
    Unhealthy = 0,
    Degraded,
    Healthy,
}

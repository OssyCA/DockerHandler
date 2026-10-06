using DockerController.Core.Models;

namespace DockerController.Core.Abstractions;

public interface IDockerHealthService
{
    Task<DaemonHealth> GetHealthAsync(CancellationToken cancellationToken);
}

using DockerController.Core.Abstractions;
using DockerController.Core.Models;

namespace DockerController.Docker.Services;

public sealed class DockerHealthService(IDockerClientAdapter adapter) : IDockerHealthService
{
    public Task<DaemonHealth> GetHealthAsync(CancellationToken cancellationToken) =>
        adapter.CheckDaemonAsync(cancellationToken);
}

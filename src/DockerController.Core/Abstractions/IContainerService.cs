using DockerController.Core.Models;
using DockerController.Core.Results;

namespace DockerController.Core.Abstractions;

public interface IContainerService
{
    IAsyncEnumerable<ContainerSummary> ListManagedAsync(CancellationToken cancellationToken);

    Task<Result<ContainerDetails>> GetAsync(string idOrName, CancellationToken cancellationToken);

    Task<Result> StartAsync(string idOrName, CancellationToken cancellationToken);

    Task<Result> StopAsync(string idOrName, int? timeoutSeconds, CancellationToken cancellationToken);

    Task<Result> RestartAsync(string idOrName, CancellationToken cancellationToken);
}

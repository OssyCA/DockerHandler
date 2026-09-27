using DockerController.Core.Models;

namespace DockerController.Core.Abstractions;

public interface IDockerClientAdapter
{
    IAsyncEnumerable<ContainerListing> ListContainersAsync(
        LabelFilter? filter,
        CancellationToken cancellationToken);

    Task<ContainerInspection?> InspectContainerAsync(
        string idOrName,
        CancellationToken cancellationToken);

    Task<bool> TryStartContainerAsync(string id, CancellationToken cancellationToken);

    Task<bool> TryStopContainerAsync(string id, int timeoutSeconds, CancellationToken cancellationToken);

    Task RestartContainerAsync(string id, int timeoutSeconds, CancellationToken cancellationToken);

    IAsyncEnumerable<ImageSummary> ListImagesAsync(CancellationToken cancellationToken);

    Task<DaemonHealth> CheckDaemonAsync(CancellationToken cancellationToken);
}

using DockerController.Core.Abstractions;
using DockerController.Core.Models;

namespace DockerController.Docker.Services;

public sealed class ImageService(IDockerClientAdapter adapter) : IImageService
{
    public IAsyncEnumerable<ImageSummary> ListAsync(CancellationToken cancellationToken) =>
        adapter.ListImagesAsync(cancellationToken);
}

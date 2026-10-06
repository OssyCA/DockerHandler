using DockerController.Core.Models;

namespace DockerController.Core.Abstractions;

public interface IImageService
{
    IAsyncEnumerable<ImageSummary> ListAsync(CancellationToken cancellationToken);
}

using DockerController.Core.Abstractions;

namespace DockerController.Api.Endpoints;

public static class ImageEndpoints
{
    public static IEndpointRouteBuilder MapImageEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/images", (IImageService images, CancellationToken cancellationToken) =>
                images.ListAsync(cancellationToken))
            .WithTags("Images")
            .WithSummary("Listar images på värden");

        return builder;
    }
}

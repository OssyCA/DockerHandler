using Docker.DotNet;
using DockerController.Core.Abstractions;
using DockerController.Core.Configuration;
using DockerController.Core.Security;
using DockerController.Docker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace DockerController.Docker.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDockerController(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton(provider =>
        {
            var options = provider.GetRequiredService<IOptions<DockerOptions>>().Value;

            return new DockerClientBuilder()
                .WithEndpoint(new Uri(options.Endpoint))
                .WithTimeout(TimeSpan.FromSeconds(options.ConnectionTimeoutSeconds))
                .Build();
        });

        services.AddSingleton<IContainerAccessPolicy>(provider =>
            new ContainerAccessPolicy(provider.GetRequiredService<IOptions<DockerOptions>>().Value));

        services.AddSingleton<IDockerClientAdapter, DockerClientAdapter>();
        services.AddSingleton<IContainerService, ContainerService>();
        services.AddSingleton<IImageService, ImageService>();
        services.AddSingleton<IDockerHealthService, DockerHealthService>();

        return services;
    }
}

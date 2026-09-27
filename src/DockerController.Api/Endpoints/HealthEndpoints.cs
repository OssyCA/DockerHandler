using DockerController.Core.Abstractions;
using DockerController.Core.Models;

namespace DockerController.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/health", async (IDockerHealthService health, CancellationToken cancellationToken) =>
            {
                var report = await health.GetHealthAsync(cancellationToken);

                return report.Status == HealthStatus.Healthy
                    ? Results.Ok(report)
                    : Results.Json(report, statusCode: StatusCodes.Status503ServiceUnavailable);
            })
            .WithTags("Health")
            .WithSummary("Controllerns hälsa")
            .WithDescription("Svarar 503 när Docker-daemonen inte går att nå.");

        return builder;
    }
}

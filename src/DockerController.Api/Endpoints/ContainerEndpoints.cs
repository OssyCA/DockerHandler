using System.ComponentModel;
using DockerController.Api.Http;
using DockerController.Core.Abstractions;
using DockerController.Core.Configuration;

namespace DockerController.Api.Endpoints;

public static class ContainerEndpoints
{
    public static IEndpointRouteBuilder MapContainerEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/containers").WithTags("Containers");

        group.MapGet("/", (IContainerService containers, CancellationToken cancellationToken) =>
                containers.ListManagedAsync(cancellationToken))
            .WithSummary("Listar hanterade containers")
            .WithDescription(
                "Strömmar de containers som bär den konfigurerade labeln och inte ligger i denylistan. " +
                "Ohanterade containers utelämnas helt.");

        group.MapGet("/{id}", async (
                string id,
                IContainerService containers,
                CancellationToken cancellationToken) =>
                (await containers.GetAsync(id, cancellationToken)).ToHttpResult())
            .WithSummary("Hämtar en hanterad container")
            .WithDescription("Id eller namn. En container som inte är hanterad svarar 404, precis som en som inte finns.");

        group.MapPost("/{id}/start", async (
                string id,
                IContainerService containers,
                CancellationToken cancellationToken) =>
                (await containers.StartAsync(id, cancellationToken)).ToHttpResult())
            .WithSummary("Startar en container")
            .WithDescription("Svarar 409 om containern redan kör.");

        group.MapPost("/{id}/stop", async (
                string id,
                [Description(StopTimeoutDescription)] int? timeout,
                IContainerService containers,
                CancellationToken cancellationToken) =>
                (await containers.StopAsync(id, timeout, cancellationToken)).ToHttpResult())
            .WithSummary("Stoppar en container")
            .WithDescription(
                $"{StopTimeoutDescription} Tillåtet intervall: {DockerOptions.MinStopTimeoutSeconds}" +
                $" till {DockerOptions.MaxStopTimeoutSeconds} sekunder. Svarar 409 om containern redan är" +
                " stoppad, och 400 om timeouten ligger utanför intervallet.");

        group.MapPost("/{id}/restart", async (
                string id,
                IContainerService containers,
                CancellationToken cancellationToken) =>
                (await containers.RestartAsync(id, cancellationToken)).ToHttpResult())
            .WithSummary("Startar om en container")
            .WithDescription("Använder den konfigurerade standardtimeouten mellan SIGTERM och SIGKILL.");

        return builder;
    }

    private const string StopTimeoutDescription =
        "Antal sekunder Docker väntar mellan SIGTERM och SIGKILL. Containern får den tiden på sig att avsluta " +
        "själv innan den dödas. 0 dödar den omedelbart. Utelämnad parameter använder serverns standardvärde.";
}

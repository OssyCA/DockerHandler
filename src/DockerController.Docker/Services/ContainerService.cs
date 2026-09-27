using System.Runtime.CompilerServices;
using DockerController.Core.Abstractions;
using DockerController.Core.Configuration;
using DockerController.Core.Models;
using DockerController.Core.Results;
using DockerController.Core.Security;
using DockerController.Core.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DockerController.Docker.Services;

public sealed class ContainerService(
    IDockerClientAdapter adapter,
    IContainerAccessPolicy policy,
    IOptions<DockerOptions> options,
    ILogger<ContainerService> logger) : IContainerService
{
    private const string NotFoundMessage = "Containern finns inte.";

    public async IAsyncEnumerable<ContainerSummary> ListManagedAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var listing in adapter.ListContainersAsync(policy.ManagedLabel, cancellationToken))
        {
            var decision = policy.Evaluate(listing.Subject);

            if (decision != AccessDecision.Allowed)
            {
                logger.LogDebug(
                    "Container {ContainerId} utelämnad ur listningen: {AccessDecision}",
                    listing.Summary.Id,
                    decision);

                continue;
            }

            yield return listing.Summary;
        }
    }

    public async Task<Result<ContainerDetails>> GetAsync(string idOrName, CancellationToken cancellationToken)
    {
        var resolved = await ResolveAsync(idOrName, cancellationToken);

        return resolved.TryGetValue(out var inspection)
            ? Result<ContainerDetails>.Success(inspection.Details)
            : Result<ContainerDetails>.NotFound(NotFoundMessage);
    }

    public async Task<Result> StartAsync(string idOrName, CancellationToken cancellationToken)
    {
        var resolved = await ResolveAsync(idOrName, cancellationToken);

        if (!resolved.TryGetValue(out var inspection))
        {
            return resolved.WithoutValue();
        }

        var outcome = await adapter.StartContainerAsync(inspection.Details.Id, cancellationToken);

        return ToResult(outcome, "Containern kör redan.");
    }

    public async Task<Result> StopAsync(
        string idOrName,
        int? timeoutSeconds,
        CancellationToken cancellationToken)
    {
        var timeout = StopTimeout.Resolve(timeoutSeconds, options.Value.DefaultStopTimeoutSeconds);

        if (!timeout.TryGetValue(out var seconds))
        {
            return timeout.WithoutValue();
        }

        var resolved = await ResolveAsync(idOrName, cancellationToken);

        if (!resolved.TryGetValue(out var inspection))
        {
            return resolved.WithoutValue();
        }

        var outcome = await adapter.StopContainerAsync(inspection.Details.Id, seconds, cancellationToken);

        return ToResult(outcome, "Containern är redan stoppad.");
    }

    public async Task<Result> RestartAsync(string idOrName, CancellationToken cancellationToken)
    {
        var resolved = await ResolveAsync(idOrName, cancellationToken);

        if (!resolved.TryGetValue(out var inspection))
        {
            return resolved.WithoutValue();
        }

        var outcome = await adapter.RestartContainerAsync(
            inspection.Details.Id,
            options.Value.DefaultStopTimeoutSeconds,
            cancellationToken);

        return outcome is ContainerOperationOutcome.NotFound
            ? Result.NotFound(NotFoundMessage)
            : Result.Success();
    }

    private async Task<Result<ContainerInspection>> ResolveAsync(
        string idOrName,
        CancellationToken cancellationToken)
    {
        var inspection = await adapter.InspectContainerAsync(idOrName, cancellationToken);

        if (inspection is null)
        {
            return Result<ContainerInspection>.NotFound(NotFoundMessage);
        }

        var decision = policy.Evaluate(inspection.Subject);

        if (decision != AccessDecision.Allowed)
        {
            logger.LogWarning(
                "Nekad åtkomst till {ContainerReference} ({ContainerId}): {AccessDecision}",
                idOrName,
                inspection.Details.Id,
                decision);

            return Result<ContainerInspection>.NotFound(NotFoundMessage);
        }

        return Result<ContainerInspection>.Success(inspection);
    }

    private static Result ToResult(ContainerOperationOutcome outcome, string conflictMessage) => outcome switch
    {
        ContainerOperationOutcome.Changed => Result.Success(),
        ContainerOperationOutcome.AlreadyInDesiredState => Result.Conflict(conflictMessage),
        _ => Result.NotFound(NotFoundMessage),
    };
}

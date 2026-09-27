using DockerController.Core.Models;
using DockerController.Core.Security;

namespace DockerController.Core.Abstractions;

// Subject hålls skilt så att labels och alias inte följer med ut i svaret.
public sealed record ContainerListing(ContainerSummary Summary, ContainerAccessSubject Subject);

public sealed record ContainerInspection(ContainerDetails Details, ContainerAccessSubject Subject);

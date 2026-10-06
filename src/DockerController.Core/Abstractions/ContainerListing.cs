using DockerController.Core.Models;
using DockerController.Core.Security;

namespace DockerController.Core.Abstractions;

public sealed record ContainerListing(ContainerSummary Summary, ContainerAccessSubject Subject);

public sealed record ContainerInspection(ContainerDetails Details, ContainerAccessSubject Subject);

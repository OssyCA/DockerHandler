using DockerController.Core.Models;
using DockerController.Core.Security;

namespace DockerController.Core.Abstractions;

public interface IContainerAccessPolicy
{
    AccessDecision Evaluate(ContainerAccessSubject subject);

    LabelFilter ManagedLabel { get; }
}

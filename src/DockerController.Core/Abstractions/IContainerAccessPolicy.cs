using DockerController.Core.Models;
using DockerController.Core.Security;

namespace DockerController.Core.Abstractions;

public interface IContainerAccessPolicy
{
    AccessDecision Evaluate(ContainerAccessSubject subject);

    // Daemon-sidans filter är bara en optimering; policyn körs ändå på varje träff.
    LabelFilter ManagedLabel { get; }
}

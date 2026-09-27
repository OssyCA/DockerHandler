using System.Collections.Frozen;
using DockerController.Core.Abstractions;
using DockerController.Core.Configuration;
using DockerController.Core.Models;

namespace DockerController.Core.Security;

public sealed class ContainerAccessPolicy : IContainerAccessPolicy
{
    private readonly string _labelKey;
    private readonly string _labelValue;
    private readonly FrozenSet<string> _deniedNames;

    public ContainerAccessPolicy(DockerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _labelKey = options.ManagedLabelKey;
        _labelValue = options.ManagedLabelValue;

        _deniedNames = options.DeniedNames
            .Select(StripNamePrefix)
            .Where(name => name.Length > 0)
            .ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }

    public LabelFilter ManagedLabel => new(_labelKey, _labelValue);

    public AccessDecision Evaluate(ContainerAccessSubject subject)
    {
        ArgumentNullException.ThrowIfNull(subject);

        foreach (var name in subject.Names)
        {
            if (_deniedNames.Contains(StripNamePrefix(name)))
            {
                return AccessDecision.DeniedByName;
            }
        }

        if (!subject.Labels.TryGetValue(_labelKey, out var value) ||
            !string.Equals(value, _labelValue, StringComparison.Ordinal))
        {
            return AccessDecision.DeniedNotManaged;
        }

        return AccessDecision.Allowed;
    }

    private static string StripNamePrefix(string name) => name.TrimStart('/').Trim();
}

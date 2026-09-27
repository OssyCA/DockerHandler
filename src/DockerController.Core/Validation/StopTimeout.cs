using DockerController.Core.Configuration;
using DockerController.Core.Results;

namespace DockerController.Core.Validation;

public static class StopTimeout
{
    // Timeouten är tiden mellan SIGTERM och SIGKILL. 0 dödar containern omedelbart.
    public static Result<int> Resolve(int? requestedSeconds, int configuredDefaultSeconds)
    {
        var seconds = requestedSeconds ?? configuredDefaultSeconds;

        if (seconds is < DockerOptions.MinStopTimeoutSeconds or > DockerOptions.MaxStopTimeoutSeconds)
        {
            return Result<int>.Invalid(
                $"Timeout måste vara mellan {DockerOptions.MinStopTimeoutSeconds} och " +
                $"{DockerOptions.MaxStopTimeoutSeconds} sekunder.");
        }

        return Result<int>.Success(seconds);
    }
}

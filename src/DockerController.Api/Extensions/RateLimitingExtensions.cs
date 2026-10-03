using System.Globalization;
using System.Threading.RateLimiting;
using DockerController.Api.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DockerController.Api.Extensions;

public static class RateLimitingExtensions
{
    private const int AuthenticatedPermitsPerMinute = 60;
    private const int AnonymousPermitsPerMinute = 10;

    public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(limiterOptions =>
        {
            limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(Partition);
            limiterOptions.OnRejected = RejectAsync;
        });

        return services;
    }

    private static RateLimitPartition<string> Partition(HttpContext httpContext)
    {
        if (httpContext.User.Identity is { IsAuthenticated: true, Name: { } keyId })
        {
            return Window($"key:{keyId}", AuthenticatedPermitsPerMinute);
        }

        var address = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return Window($"ip:{address}", AnonymousPermitsPerMinute);
    }

    private static RateLimitPartition<string> Window(string partitionKey, int permitLimit) =>
        RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0,
        });

    private static async ValueTask RejectAsync(OnRejectedContext context, CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(CultureInfo.InvariantCulture);
        }

        await httpContext.RequestServices
            .GetRequiredService<IProblemDetailsService>()
            .TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status429TooManyRequests,
                    Title = "För många anrop.",
                    Extensions = { [ErrorCodes.PropertyName] = ErrorCodes.RateLimited },
                },
            });
    }
}

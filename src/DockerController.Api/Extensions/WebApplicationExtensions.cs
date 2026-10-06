using DockerController.Core.Configuration;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;

namespace DockerController.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(loggingOptions =>
            loggingOptions.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                if (httpContext.User.Identity is { IsAuthenticated: true, Name: { } keyId })
                {
                    diagnosticContext.Set("ApiKeyId", keyId);
                }
            });

        return app;
    }

    public static WebApplication MapDeveloperDocumentation(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return app;
        }

        app.MapOpenApi().AllowAnonymous();
        app.MapScalarApiReference().AllowAnonymous();
        app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription().AllowAnonymous();

        return app;
    }

    public static WebApplication WarnOnEmptyDenylist(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<DockerOptions>>().Value;

        if (options.DeniedNames.Length == 0)
        {
            app.Logger.LogWarning(
                "DeniedNames är tom. Controllerns egen container bör ligga där, annars kan API:et stoppa sig självt.");
        }

        return app;
    }
}

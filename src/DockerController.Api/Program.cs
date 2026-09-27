using DockerController.Api.Configuration;
using DockerController.Core.Abstractions;
using DockerController.Core.Configuration;
using DockerController.Core.Security;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddSerilog((services, configuration) => configuration
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services
        .AddOptions<DockerOptions>()
        .Bind(builder.Configuration.GetSection(DockerOptions.SectionName))
        .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<DockerOptions>, DockerOptionsValidator>();

    builder.Services
        .AddOptions<ApiAuthOptions>()
        .Bind(builder.Configuration.GetSection(ApiAuthOptions.SectionName))
        .ValidateOnStart();
    builder.Services.AddSingleton<IValidateOptions<ApiAuthOptions>, ApiAuthOptionsValidator>();

    builder.Services.AddSingleton<IContainerAccessPolicy>(provider =>
        new ContainerAccessPolicy(provider.GetRequiredService<IOptions<DockerOptions>>().Value));

    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

    // TODO fas 1: DockerClientAdapter, tjänsteregistrering, endpoints, ResultMapper,
    // IExceptionHandler med ProblemDetails.

    app.Run();
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "Applikationen kunde inte starta");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

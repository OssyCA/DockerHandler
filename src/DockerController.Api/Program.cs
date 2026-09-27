using System.Text.Json.Serialization;
using DockerController.Api.Configuration;
using DockerController.Api.Endpoints;
using DockerController.Api.Http;
using DockerController.Core.Configuration;
using DockerController.Docker.DependencyInjection;
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

    builder.Services.AddDockerController();

    builder.Services.ConfigureHttpJsonOptions(jsonOptions =>
        jsonOptions.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

    builder.Services.AddProblemDetails();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar")).ExcludeFromDescription();

    app.MapContainerEndpoints();
    app.MapImageEndpoints();
    app.MapHealthEndpoints();

    WarnOnEmptyDenylist(app);

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

static void WarnOnEmptyDenylist(WebApplication app)
{
    var options = app.Services.GetRequiredService<IOptions<DockerOptions>>().Value;

    if (options.DeniedNames.Length == 0)
    {
        app.Logger.LogWarning(
            "DeniedNames är tom. Controllerns egen container bör ligga där, annars kan API:et stoppa sig självt.");
    }
}

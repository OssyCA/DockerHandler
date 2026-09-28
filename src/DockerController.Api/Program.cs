using DockerController.Api.Endpoints;
using DockerController.Api.Extensions;
using DockerController.Docker.DependencyInjection;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services.AddApiLogging(builder.Configuration);
    builder.Services.AddApiOptions(builder.Configuration);
    builder.Services.AddApiKeyAuthentication();
    builder.Services.AddApiRateLimiting();
    builder.Services.AddApiConventions();
    builder.Services.AddDockerController();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseApiRequestLogging();

    app.UseAuthentication();
    app.UseRateLimiter();
    app.UseAuthorization();

    app.MapDeveloperDocumentation();

    app.MapContainerEndpoints();
    app.MapImageEndpoints();
    app.MapHealthEndpoints();

    app.WarnOnEmptyDenylist();

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

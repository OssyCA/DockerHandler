using System.Security.Claims;
using System.Text.Encodings.Web;
using DockerController.Api.Http;
using DockerController.Core.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DockerController.Api.Security;

public sealed class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "ApiKey";
    public const string HeaderName = "X-Api-Key";

    private readonly ApiKeyRegistry _registry;
    private readonly IProblemDetailsService _problemDetailsService;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ApiKeyRegistry registry,
        IProblemDetailsService problemDetailsService)
        : base(options, loggerFactory, encoder)
    {
        _registry = registry;
        _problemDetailsService = problemDetailsService;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(HeaderName, out var header))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var presented = header.ToString();

        if (presented.Length == 0)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!_registry.TryResolve(presented, out var keyId))
        {
            Logger.LogWarning(
                "Ogiltig API-nyckel mot {Method} {Path} från {RemoteIpAddress}",
                Request.Method,
                Request.Path,
                Context.Connection.RemoteIpAddress);

            return Task.FromResult(AuthenticateResult.Fail("Ogiltig API-nyckel."));
        }

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, keyId)], Scheme.Name);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;

        await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = Context,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Giltig API-nyckel krävs.",
                Extensions = { [ErrorCodes.PropertyName] = ErrorCodes.Unauthorized },
            },
        });
    }
}

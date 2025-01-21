using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KittySaver.Api.Tests.Integration.Helpers;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Here we set up the claims that your application needs
        Claim[] claims = [
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Role, "SuperAdmin"),
            new(ClaimTypes.Email, "defaultadmin@koniec.dev")
        ];

        ClaimsIdentity identity = new(claims, "TestAuthType");
        ClaimsPrincipal principal = new(identity);
        AuthenticationTicket ticket = new(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
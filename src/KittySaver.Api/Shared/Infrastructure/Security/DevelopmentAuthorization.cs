using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace KittySaver.Api.Shared.Infrastructure.Security;

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
            new Claim(ClaimTypes.NameIdentifier, Guid.Parse("a4018ea1-525a-48eb-a701-a96c1a261e72").ToString()),
            new Claim(ClaimTypes.Role, "Administrator"),
            new Claim(ClaimTypes.Email, "defaultadmin@koniec.dev")
        ];

        ClaimsIdentity identity = new ClaimsIdentity(claims, "DevScheme");
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, "DevScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace KittySaver.Auth.Api.Swagger.Auth;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        Claim[] claims = [
            new(ClaimTypes.NameIdentifier, FixedIdsHelper.AdminId.ToString()),
            new(ClaimTypes.Role, "Administrator"),
            new(ClaimTypes.Email, "defaultadmin@koniec.dev")
        ];

        ClaimsIdentity identity = new ClaimsIdentity(claims, "DevScheme");
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, "DevScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}


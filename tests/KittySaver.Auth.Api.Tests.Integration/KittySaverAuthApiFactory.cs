using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Persistence;
using KittySaver.Auth.Api.Shared.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Testcontainers.MsSql;

namespace KittySaver.Auth.Api.Tests.Integration;
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
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "SuperAdmin"),
            new Claim(ClaimTypes.Email, "defaultadmin@koniec.dev")
        ];

        ClaimsIdentity identity = new ClaimsIdentity(claims, "TestAuthType");
        ClaimsPrincipal principal = new ClaimsPrincipal(identity);
        AuthenticationTicket ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public class KittySaverAuthApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer
        = new MsSqlBuilder().Build();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            ServiceDescriptor? descriptor = services.SingleOrDefault(m => m.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if(descriptor is not null)
            {
                services.Remove(descriptor);
            }
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString());
            });
            
            ServiceDescriptor? clientDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IKittySaverApiClient) &&
                     d.ImplementationType == typeof(KittySaverApiClient));

            if (clientDescriptor is not null)
            {
                services.Remove(clientDescriptor);
            }
            
            services.RemoveAll(typeof(IAuthenticationService));
            services.RemoveAll(typeof(IAuthorizationHandler));
            AddTestSchemeAuth();
            
            IKittySaverApiClient mockKittySaverApiClient = Substitute.For<IKittySaverApiClient>();
            
            mockKittySaverApiClient
                .CreatePerson(Arg.Any<IKittySaverApiClient.CreatePersonDto>())
                .Returns(callInfo => {
                    IKittySaverApiClient.CreatePersonDto? createPersonDto = callInfo.Arg<IKittySaverApiClient.CreatePersonDto>();
                    if (createPersonDto.Email == "apiFactoryWill@Throw.IntServErr") {
                        throw new Exception("Internal Server Error");
                    }
                    return Guid.NewGuid();
                });
            
            services.AddSingleton(mockKittySaverApiClient);
            return;
            
            void AddTestSchemeAuth()
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "TestScheme";
                    options.DefaultChallengeScheme = "TestScheme";
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

                services.AddAuthorizationBuilder()
                    .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                        .AddAuthenticationSchemes("TestScheme")
                        .RequireAssertion(_ => true)
                        .Build());
            }
        });
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _msSqlContainer.DisposeAsync();
    }
}
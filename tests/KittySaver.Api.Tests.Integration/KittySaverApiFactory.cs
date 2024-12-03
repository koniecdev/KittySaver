using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Shared;
using Testcontainers.MsSql;

namespace KittySaver.Api.Tests.Integration;

public class KittySaverApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer
        = new MsSqlBuilder().Build();

    public static DateTimeOffset FixedDateTime => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
    public static int FixedMinutesJwtExpire => 5;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, configBuilder) =>
        {
            // Remove the default configuration options
            configBuilder.Sources.Clear();

            // Add custom configuration file(s) for your tests
            configBuilder.AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Test.json", optional: true)
                .AddEnvironmentVariables();

            // You can also add in-memory configuration for overriding specific values
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "AppSettings:MinutesTokenExpiresIn", $"{FixedMinutesJwtExpire}" }
            });
        });
        
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IDateTimeProvider));
            IDateTimeService dateTimeSub = new ApplicationDateTimeService();
            services.AddSingleton<IDateTimeService>(_ => dateTimeSub);
            
            services.RemoveAll(typeof(IAuthenticationService));
            services.RemoveAll(typeof(IAuthorizationHandler));
            
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
            
            ServiceDescriptor? descriptor = services.SingleOrDefault(m => m.ServiceType == typeof(DbContextOptions<ApplicationWriteDbContext>));
            if(descriptor is not null)
            {
                services.Remove(descriptor);
            }
            services.AddDbContext<ApplicationWriteDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString());
            });
            
            //.BaseAddress = new Uri(_authApiServer.Url)
            
        });
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        using IServiceScope scope = Services.CreateScope();
        ApplicationWriteDbContext writeDbContext = scope.ServiceProvider.GetRequiredService<ApplicationWriteDbContext>();
        await writeDbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        // _authApiServer.Dispose();
        await _msSqlContainer.DisposeAsync();
    }
}

public class ApplicationDateTimeService : IDateTimeService
{
    public static DateTimeOffset ApplicationDateTime { get; set; } = new(2024, 2, 10, 1, 1, 1, TimeSpan.Zero);

    public DateTimeOffset Now
    {
        get
        {
            ApplicationDateTime = ApplicationDateTime.AddMonths(2);
            return ApplicationDateTime;
        }
    }
}
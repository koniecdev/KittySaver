using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Tests.Integration.Helpers;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Domain.Persons;
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
using NSubstitute;
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
        builder.UseEnvironment("Testing");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
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
                { "AppSettings:MinutesTokenExpiresIn", $"{FixedMinutesJwtExpire}" },
                { "ASPNETCORE_ENVIRONMENT", "Testing" } // Add this line
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
            
            services.RemoveAll(typeof(ICurrentUserService));
            ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
            currentUserService.EnsureUserIsAdminAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
            currentUserService.EnsureUserIsAuthorizedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            currentUserService.GetCurrentUserIdentityId().Returns(Guid.NewGuid());
            currentUserService.GetCurrentlyLoggedInPersonAsync(Arg.Any<CancellationToken>()).Returns(
                Task.FromResult<CurrentlyLoggedInPerson?>
                    (new CurrentlyLoggedInPerson{ PersonId = Guid.NewGuid(), Role = Person.Role.Admin}));
            services.AddSingleton<ICurrentUserService>(_ => currentUserService);
            
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
            
            ServiceDescriptor? descriptorOfReadDbContext = services.SingleOrDefault(m => m.ServiceType == typeof(DbContextOptions<ApplicationReadDbContext>));
            if(descriptorOfReadDbContext is not null)
            {
                services.Remove(descriptorOfReadDbContext);
            }
            services.AddDbContext<ApplicationReadDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString()).UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
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
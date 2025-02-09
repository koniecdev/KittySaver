using KittySaver.Api.Shared.Infrastructure.Clients;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Tests.Integration.Helpers;
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
using Testcontainers.MsSql;

namespace KittySaver.Api.Tests.Integration;

public class KittySaverApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer
        = new MsSqlBuilder().Build();

    private static int FixedMinutesJwtExpire => 5;
    
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
            services.RemoveAll<IDateTimeService>();
            IDateTimeService dateTimeSub = new ApplicationDateTimeService();
            services.AddSingleton<IDateTimeService>(_ => dateTimeSub);
            
            services.RemoveAll<IAdvertisementFileStorageService>();
            IAdvertisementFileStorageService advertisementFileStorageService = Substitute.For<IAdvertisementFileStorageService>();
            TestableFileStream testStream = new("test/path/image.jpg");
            advertisementFileStorageService
                .GetThumbnail(Arg.Any<Guid>())
                .Returns(testStream);

            advertisementFileStorageService
                .GetContentType("test/path/image.jpg")
                .Returns("image/jpeg");
            services.AddSingleton<IAdvertisementFileStorageService>(_ => advertisementFileStorageService);
            
            services.RemoveAll<ICatFileStorageService>();
            ICatFileStorageService catFileStorageService = Substitute.For<ICatFileStorageService>();
            TestableFileStream testStream1 = new("test/path/image.jpg");
            catFileStorageService
                .GetThumbnail(Arg.Any<Guid>())
                .Returns(testStream1);

            catFileStorageService
                .GetContentType("test/path/image.jpg")
                .Returns("image/jpeg");
            services.AddSingleton<ICatFileStorageService>(_ => catFileStorageService);
            
            services.RemoveAll<ICurrentUserService>();
            ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
            currentUserService.EnsureUserIsAdminAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(true));
            currentUserService.EnsureUserIsAuthorizedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            currentUserService.GetCurrentUserIdentityId().Returns(Guid.NewGuid());
            currentUserService.GetCurrentlyLoggedInPersonAsync(Arg.Any<CancellationToken>()).Returns(
                Task.FromResult<CurrentlyLoggedInPerson?>
                    (new CurrentlyLoggedInPerson { PersonId = Guid.NewGuid(), Role = Person.Role.Admin}));
            services.AddSingleton<ICurrentUserService>(_ => currentUserService);
            
            services.RemoveAll<IAuthenticationService>();
            services.RemoveAll<IAuthorizationHandler>();
            
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
            
            ServiceDescriptor? descriptor = services
                .SingleOrDefault(m => m.ServiceType == typeof(DbContextOptions<ApplicationWriteDbContext>));
            if(descriptor is not null)
            {
                services.Remove(descriptor);
            }
            services.AddDbContext<ApplicationWriteDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString());
            });
            
            ServiceDescriptor? clientDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IAuthApiHttpClient) &&
                     d.ImplementationType == typeof(IAuthApiHttpClient));

            if (clientDescriptor is not null)
            {
                services.Remove(clientDescriptor);
            }
            
            IAuthApiHttpClient mockKittySaverApiClient = Substitute.For<IAuthApiHttpClient>();
            mockKittySaverApiClient.RegisterAsync(Arg.Any<IAuthApiHttpClient.RegisterDto>()).Returns(Guid.NewGuid());
            services.AddSingleton(mockKittySaverApiClient);
            
            ServiceDescriptor? descriptorOfReadDbContext = services
                .SingleOrDefault(m => m.ServiceType == typeof(DbContextOptions<ApplicationReadDbContext>));
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
        await _msSqlContainer.DisposeAsync();
    }
}

public class ApplicationDateTimeService : IDateTimeService
{
    private static DateTimeOffset ApplicationDateTime { get; set; } = new(2024, 2, 10, 1, 1, 1, TimeSpan.Zero);

    public DateTimeOffset Now
    {
        get
        {
            ApplicationDateTime = ApplicationDateTime.AddMonths(2);
            return ApplicationDateTime;
        }
    }
}

public class TestableFileStream(string path) : FileStream(Path.GetTempFileName(), FileMode.Open)
{
    public new string Name => TestName;
    private string TestName { get; } = path;
}

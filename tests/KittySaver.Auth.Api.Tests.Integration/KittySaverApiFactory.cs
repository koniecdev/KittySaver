using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Testcontainers.MsSql;

namespace KittySaver.Auth.Api.Tests.Integration;

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
            
            var clientDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IKittySaverApiClient) &&
                     d.ImplementationType == typeof(KittySaverApiClient));

            if (clientDescriptor is not null)
            {
                services.Remove(clientDescriptor);
            }

            // Create a mock instance of IKittySaverApiClient
            IKittySaverApiClient mockKittySaverApiClient = Substitute.For<IKittySaverApiClient>();

            
            mockKittySaverApiClient
                .CreatePerson(Arg.Any<IKittySaverApiClient.CreatePersonDto>())
                .Returns(callInfo => {
                    var createPersonDto = callInfo.Arg<IKittySaverApiClient.CreatePersonDto>();
                    if (createPersonDto.Email == "apiFactoryWill@Throw.IntServErr") {
                        throw new Exception("Internal Server Error");
                    }
                    return Guid.NewGuid();
                });
            
            // Replace the IKittySaverApiClient with the mocked instance
            services.AddSingleton(mockKittySaverApiClient);
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
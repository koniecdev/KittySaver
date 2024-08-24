using KittySaver.Api.Shared.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;

namespace KittySaver.Api.Tests.Integration;

public class KittySaverApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer
        = new MsSqlBuilder().Build();

    // private readonly AuthApiServer _authApiServer = new();
    
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
            
            //.BaseAddress = new Uri(_authApiServer.Url)
            
        });
    }

    public async Task InitializeAsync()
    {
        await _msSqlContainer.StartAsync();
        // _authApiServer.Start();
        using IServiceScope scope = Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        // _authApiServer.Dispose();
        await _msSqlContainer.DisposeAsync();
    }
}
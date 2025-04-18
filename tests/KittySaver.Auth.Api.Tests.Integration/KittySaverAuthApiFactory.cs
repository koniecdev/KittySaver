﻿using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Auth.Api.Shared.Infrastructure.Services.Email;
using KittySaver.Auth.Api.Shared.Persistence;
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
using TestAuthHandler = KittySaver.Auth.Api.Tests.Integration.Helpers.TestAuthHandler;

namespace KittySaver.Auth.Api.Tests.Integration;

public class KittySaverAuthApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly MsSqlContainer _msSqlContainer
        = new MsSqlBuilder().Build();
    
    public const string DefaultAdminEmail = "defaultadmin@koniec.dev";
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
            services.RemoveAll<IDateTimeProvider>();
            services.RemoveAll<ICurrentUserService>();
            services.RemoveAll<IEmailService>();
            IEmailService emailServiceStub = Substitute.For<IEmailService>();
            IDateTimeProvider dateTimeSub = Substitute.For<IDateTimeProvider>();
            ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
            currentUserService.UserId.Returns(Guid.NewGuid().ToString());
            currentUserService.UserRoles.Returns(["Admin"]);
            
            dateTimeSub.Now.Returns(FixedDateTime);
            services.AddScoped<IDateTimeProvider>(_ => dateTimeSub);
            services.AddScoped<ICurrentUserService>(_ => currentUserService);
            services.AddScoped<IEmailService>(_ => emailServiceStub);
            
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
            
            ServiceDescriptor? descriptor = services.SingleOrDefault(m => m.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if(descriptor is not null)
            {
                services.Remove(descriptor);
            }
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(_msSqlContainer.GetConnectionString());
            });
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
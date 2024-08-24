using System.Reflection;
using FluentValidation;
using KittySaver.Auth.Api.Shared.Behaviours;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Auth.Api.Shared.Persistence;
using KittySaver.Auth.Api.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Auth.Api.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentEnvironmentService, CurrentEnvironmentService>();
        services.AddScoped<IDateTimeProvider, DefaultDateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        services.AddHttpClient<IKittySaverApiClient, KittySaverApiClient>(httpClient =>
        {
            httpClient.BaseAddress = new Uri(configuration.GetValue<string>("Api:ApiBaseUrl")
                                             ?? throw new Exception("ApiBaseUrl not found in appsettings"));
        });
        return services;
    }
    public static IServiceCollection RegisterPersistenceServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(
            o => o.UseSqlServer(configuration.GetConnectionString("Database")
                                ?? throw new Exceptions.Database.MissingConnectionStringException()));
        services
            .AddIdentityCore<ApplicationUser>(x =>
            {
                x.Password.RequireNonAlphanumeric = false;
                x.Password.RequireDigit = true;
                x.Password.RequiredLength = 8;
                x.Password.RequireLowercase = true;
                x.Password.RequireUppercase = true;
                x.Password.RequiredUniqueChars = 0;
            }).AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        return services;
    }

    public static class Exceptions
    {
        public static class Database
        {
            public sealed class MissingConnectionStringException() : Exception("Connection string not found");
        }
    } 
}
using System.Reflection;
using FluentValidation;
using KittySaver.Api.Shared.Behaviours;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KittySaver.Api.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentEnvironmentService, CurrentEnvironmentService>();
        services.AddScoped<IDateTimeProvider, DefaultDateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        return services;
    }
    public static IServiceCollection RegisterPersistenceServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(
            o => o.UseSqlServer(configuration.GetConnectionString("Database")
                                ?? throw new Exceptions.Database.MissingConnectionStringException()));
        services.AddIdentityCore<Person>()
            .AddRoles<IdentityRole<Guid>>()
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
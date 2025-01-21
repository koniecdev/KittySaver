using System.Reflection;
using System.Text;
using FluentValidation;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Behaviours;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Pagination;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace KittySaver.Api.Shared.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        services.AddHttpContextAccessor();
        services.AddDbContext<ApplicationReadDbContext>(o => o
            .UseSqlServer(configuration.GetConnectionString("Database") ?? throw new Exceptions.Database.MissingConnectionStringException())
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
        services.AddScoped<ILinkService, LinkService>();
        services.AddScoped<IPaginationLinksService, PaginationLinksService>();
        services.AddScoped<ICurrentEnvironmentService, CurrentEnvironmentService>();
        services.AddScoped<IDateTimeService, DefaultDateTimeService>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IPersonUniquenessChecksRepository, PersonUniquenessChecksRepository>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICatPriorityCalculatorService, DefaultCatPriorityCalculatorService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IThumbnailStorageService, ThumbnailStorageService>();
        services.AddScoped<IAdvertisementFileStorageService, AdvertisementFileStorageService>();
        services.AddScoped<ICatFileStorageService, CatFileStorageService>();
        services.AddValidatorsFromAssembly(assembly);
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Token"] ?? throw new InvalidConfigurationException("JWT Token not found in appsettings!!")))
                };
            });
        services.AddAuthorization();
        
        services.AddDbContext<ApplicationWriteDbContext>(o =>
            o.UseSqlServer(configuration.GetConnectionString("Database") ?? throw new Exceptions.Database.MissingConnectionStringException()));
        
        services.AddScoped<IUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<ApplicationWriteDbContext>());
        
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(AuthorizationBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(HateoasBehaviour<,>));
            cfg.AddOpenBehavior(typeof(PagedHateoasBehaviour<,>));
        });
        return services;
    }

    private static class Exceptions
    {
        public static class Database
        {
            public sealed class MissingConnectionStringException() : Exception("Connection string not found");
        }
    } 
}
using System.Reflection;
using System.Text;
using FluentValidation;
using KittySaver.Auth.Api.Shared.Behaviours;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Auth.Api.Shared.Persistence;
using KittySaver.Auth.Api.Shared.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KittySaver.Auth.Api.Shared.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
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
        AddAuth();
        
        return services;

        void AddAuth()
        {
            if (environment.IsDevelopment())
            {
                AddDevSchemeAuth();
            }
            else
            {
                AddJwtAuth();
            }
        }
        
        void AddDevSchemeAuth()
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "DevScheme";
                options.DefaultChallengeScheme = "DevScheme";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("DevScheme", _ => { });
            
            services.AddAuthorizationBuilder()
                .SetDefaultPolicy(new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes("DevScheme")
                    .RequireAssertion(_ => true)
                    .Build());
        }

        void AddJwtAuth()
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(x =>
                {
                    x.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Token"]!))
                    };
                });
            services.AddAuthorization();
        }
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
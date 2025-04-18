﻿using System.Reflection;
using System.Text;
using FluentValidation;
using KittySaver.Auth.Api.Shared.Behaviours;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.Services;
using KittySaver.Auth.Api.Shared.Infrastructure.Services.Email;
using KittySaver.Auth.Api.Shared.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace KittySaver.Auth.Api.Shared.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentEnvironmentService, CurrentEnvironmentService>();
        services.AddScoped<IDateTimeProvider, DefaultDateTimeProvider>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddValidatorsFromAssembly(assembly);
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
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

        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        EmailSettings emailSettings = configuration.GetSection("EmailSettings").Get<EmailSettings>()
                                      ?? throw new Exception("Missing email settings");
        services
            .AddFluentEmail(emailSettings.SenderEmail, emailSettings.SenderName)
            .AddRazorRenderer()
            .AddSmtpSender(new System.Net.Mail.SmtpClient
            {
                Host = emailSettings.Server,
                Port = emailSettings.Port,
                EnableSsl = emailSettings.UseSsl,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(emailSettings.Username, emailSettings.Password)
            });
        
        services.AddScoped<IEmailService, EmailService>();
        return services;
    }
    public static IServiceCollection RegisterPersistenceServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(
            o => o.UseSqlServer(configuration.GetConnectionString("Database")
                                ?? throw new Exceptions.Database.MissingConnectionStringException()));
        services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredUniqueChars = 0;
            
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.SignIn.RequireConfirmedAccount = true;
            
                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddEntityFrameworkStores<ApplicationDbContext>();
    
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
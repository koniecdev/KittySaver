﻿using FluentValidation;
using KittySaver.Auth.Api.Features.ApplicationUsers.SharedContracts;
using KittySaver.Auth.Api.Shared.Abstractions;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Endpoints;
using KittySaver.Auth.Api.Shared.Exceptions;
using KittySaver.Auth.Api.Shared.Infrastructure.Services.Email;
using KittySaver.Auth.Api.Shared.Persistence;
using KittySaver.Shared.Requests;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Riok.Mapperly.Abstractions;
using System.Text;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Register : IEndpoint
{
    public sealed record RegisterCommand(
        string UserName,
        string Email,
        string PhoneNumber,
        string Password) : ICommand<Guid>;

    public sealed class RegisterCommandValidator
        : AbstractValidator<RegisterCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;

        public RegisterCommandValidator(ApplicationDbContext db)
        {
            _db = db;
            RuleFor(x => x.Password)
                .NotEmpty();
            RuleFor(x => x.Password)
                .MinimumLength(8)
                .WithMessage("'Password' is not in the correct format. Your password length must be at least 8.")
                .Matches("[A-Z]+")
                .WithMessage("'Password' is not in the correct format. Your password must contain at least one uppercase letter.")
                .Matches("[a-z]+")
                .WithMessage("'Password' is not in the correct format. Your password must contain at least one lowercase letter.")
                .Matches("[0-9]+")
                .WithMessage("'Password' is not in the correct format. Your password must contain at least one number.");
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern)
                .MustAsync(async (email, ct) => await IsEmailUniqueAsync(email, ct))
                .WithMessage("Email is already used by another user.");
        }
        
        private async Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct) 
            => !await _db.ApplicationUsers
                .AsNoTracking()
                .AnyAsync(x=>x.Email == email, ct);
    }
    
    internal sealed class RegisterCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IOptions<EmailSettings> emailSettings)
        : IRequestHandler<RegisterCommand, Guid>
    {
        public async Task<Guid> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = request.ToEntity();
            IdentityResult result = await userManager.CreateAsync(user, request.Password);
            
            if (!result.Succeeded)
            {
                string errorMessage = result.Errors.FirstOrDefault()?.Description ?? "User creation failed";
                throw new InvalidOperationException(errorMessage);
            }
            
            string token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            string encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            
            string confirmationLink = $"{emailSettings.Value.WebsiteBaseUrl}/confirm-email?userId={user.Id}&token={encodedToken}";
            
            await emailService.SendEmailConfirmationAsync(user.Email!, confirmationLink);
            
            return user.Id;
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("application-users", async
            (RegisterRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            RegisterCommand command = request.ToRegisterCommand();
            Guid personId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/application-users/{personId}", new { Id = personId });
        });
    }
}

[Mapper]
public static partial class RegisterMapper
{
    public static partial Register.RegisterCommand ToRegisterCommand(this RegisterRequest request);
    public static partial ApplicationUser ToEntity(this Register.RegisterCommand command);
}
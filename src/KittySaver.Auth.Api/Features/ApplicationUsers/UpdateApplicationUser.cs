using FluentValidation;
using KittySaver.Auth.Api.Shared.Domain.Entites;
using KittySaver.Auth.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Auth.Api.Shared.Infrastructure.Clients;
using KittySaver.Auth.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Auth.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Auth.Api.Features.ApplicationUsers;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class UpdateApplicationUser : IEndpoint
{
    public sealed record UpdateApplicationUserRequest(
        string UserName,
        string DefaultAdvertisementPickupAddressCountry,
        string DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string DefaultAdvertisementPickupAddressStreet,
        string DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber);
    
    public sealed record UpdateApplicationUserCommand(
        Guid Id,
        string UserName,
        string DefaultAdvertisementPickupAddressCountry,
        string DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string DefaultAdvertisementPickupAddressStreet,
        string DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber) : ICommand;

    public sealed class UpdateApplicationUserCommandValidator
        : AbstractValidator<UpdateApplicationUserCommand>
    {
        public UpdateApplicationUserCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressCountry).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressState).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressZipCode).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressCity).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressStreet).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementPickupAddressBuildingNumber).NotEmpty();
            RuleFor(x => x.DefaultAdvertisementContactInfoEmail).NotEmpty().EmailAddress();
            RuleFor(x => x.DefaultAdvertisementContactInfoPhoneNumber).NotEmpty();
        }
    }
    
    internal sealed class UpdateApplicationUserCommandHandler(
        ApplicationDbContext db,
        IKittySaverApiClient client)
        : IRequestHandler<UpdateApplicationUserCommand>
    {
        public async Task Handle(UpdateApplicationUserCommand request, CancellationToken cancellationToken)
        {
            ApplicationUser user = await db.ApplicationUsers
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new ApplicationUser.Exceptions.ApplicationUserNotFoundException();
            UpdateApplicationUserMapper.UpdateEntityWithCommandData(request, user);
            IKittySaverApiClient.UpdatePersonDto command = new IKittySaverApiClient.UpdatePersonDto(
                user.UserName!,
                user.Email!,
                user.PhoneNumber!,
                user.DefaultAdvertisementPickupAddressCountry,
                user.DefaultAdvertisementPickupAddressState,
                user.DefaultAdvertisementPickupAddressZipCode,
                user.DefaultAdvertisementPickupAddressCity,
                user.DefaultAdvertisementPickupAddressStreet,
                user.DefaultAdvertisementPickupAddressBuildingNumber,
                user.DefaultAdvertisementContactInfoEmail,
                user.DefaultAdvertisementContactInfoPhoneNumber);
            await client.UpdatePerson(user.Id, command);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("application-users/{id:guid}", async
            (Guid id, 
            UpdateApplicationUserRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UpdateApplicationUserCommand command = request.ToUpdateCommand(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class UpdateApplicationUserMapper
{
    public static partial UpdateApplicationUser.UpdateApplicationUserCommand ToUpdateCommand(this UpdateApplicationUser.UpdateApplicationUserRequest request, Guid id);
    [MapperIgnoreTarget(nameof(ApplicationUser.Email))]
    [MapperIgnoreTarget(nameof(ApplicationUser.PhoneNumber))]
    public static partial void UpdateEntityWithCommandData(UpdateApplicationUser.UpdateApplicationUserCommand source,
        ApplicationUser target);
}

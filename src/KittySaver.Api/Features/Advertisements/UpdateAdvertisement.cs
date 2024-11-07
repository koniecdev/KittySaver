using FluentValidation;
using KittySaver.Api.Shared.Domain.Common.Interfaces;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.ValueObjects;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Advertisements;

public sealed class UpdateAdvertisement : IEndpoint
{
    public sealed record UpdateAdvertisementRequest(
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string? PickupAddressStreet,
        string? PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber);

    public sealed record UpdateAdvertisementCommand(
        Guid Id,
        Guid PersonId,
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string? PickupAddressStreet,
        string? PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber) : ICommand;

    public sealed class UpdateAdvertisementCommandValidator
        : AbstractValidator<UpdateAdvertisementCommand>
    {
        public UpdateAdvertisementCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.PersonId).NotEmpty();
            
            RuleFor(x => x.Description).MaximumLength(Advertisement.Constraints.DescriptionMaxLength);
            
            RuleFor(x => x.ContactInfoPhoneNumber)
                .NotEmpty()
                .MaximumLength(IContact.Constraints.PhoneNumberMaxLength);

            RuleFor(x => x.ContactInfoEmail)
                .NotEmpty()
                .MaximumLength(IContact.Constraints.EmailMaxLength)
                .Matches(IContact.Constraints.EmailPattern);
            
            RuleFor(x => x.PickupAddressCountry)
                .NotEmpty()
                .MaximumLength(IAddress.Constraints.CountryMaxLength);
            
            RuleFor(x => x.PickupAddressState)
                .MaximumLength(IAddress.Constraints.StateMaxLength);
            
            RuleFor(x => x.PickupAddressZipCode)
                .NotEmpty()
                .MaximumLength(IAddress.Constraints.ZipCodeMaxLength);
            
            RuleFor(x => x.PickupAddressCity)
                .NotEmpty()
                .MaximumLength(IAddress.Constraints.CityMaxLength);
            
            RuleFor(x => x.PickupAddressStreet)
                .MaximumLength(IAddress.Constraints.StreetMaxLength);
            
            RuleFor(x => x.PickupAddressBuildingNumber)
                .MaximumLength(IAddress.Constraints.BuildingNumberMaxLength);
        }
    }

    internal sealed class UpdateAdvertisementCommandHandler(ApplicationDbContext db)
        : IRequestHandler<UpdateAdvertisementCommand>
    {
        public async Task Handle(UpdateAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement = await db.Advertisements
                                .Where(x => 
                                    x.Id == request.Id
                                    && x.PersonId == request.PersonId)
                                .FirstOrDefaultAsync(cancellationToken)
                            ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            
            PickupAddress pickupAddress = new()
            {
                Country = request.PickupAddressCountry,
                State = request.PickupAddressState,
                ZipCode = request.PickupAddressZipCode,
                City = request.PickupAddressCity,
                Street = request.PickupAddressStreet,
                BuildingNumber = request.PickupAddressBuildingNumber
            };

            ContactInfo contactInfo = new()
            {
                Email = request.ContactInfoEmail,
                PhoneNumber = request.ContactInfoPhoneNumber
            };
            
            advertisement.PickupAddress = pickupAddress;
            advertisement.ContactInfo = contactInfo;
            advertisement.Description = request.Description;
            
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{id:guid}", async (
            Guid id,
            UpdateAdvertisementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UpdateAdvertisementCommand command = request.MapToUpdateAdvertisementCommand(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        });
    }
}

[Mapper]
public static partial class UpdateAdvertisementMapper
{
    public static partial UpdateAdvertisement.UpdateAdvertisementCommand MapToUpdateAdvertisementCommand(
        this UpdateAdvertisement.UpdateAdvertisementRequest request,
        Guid idOrUserIdentityId);
}
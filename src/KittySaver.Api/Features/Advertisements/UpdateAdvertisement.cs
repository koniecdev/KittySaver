using FluentValidation;
using KittySaver.Api.Shared.Domain.Advertisement;
using KittySaver.Api.Shared.Domain.Persons;
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
        string PickupAddressStreet,
        string PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber);

    public sealed record UpdateAdvertisementCommand(
        Guid Id,
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string PickupAddressStreet,
        string PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber) : ICommand;

    public sealed class UpdateAdvertisementCommandValidator
        : AbstractValidator<UpdateAdvertisementCommand>
    {
        public UpdateAdvertisementCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            
            RuleFor(x => x.Description).MaximumLength(Description.MaxLength);
            
            RuleFor(x => x.ContactInfoPhoneNumber)
                .NotEmpty()
                .MaximumLength(PhoneNumber.MaxLength);

            RuleFor(x => x.ContactInfoEmail)
                .NotEmpty()
                .MaximumLength(Email.MaxLength)
                .Matches(Email.RegexPattern);
            
            RuleFor(x => x.PickupAddressCountry)
                .NotEmpty()
                .MaximumLength(Address.CountryMaxLength);
            
            RuleFor(x => x.PickupAddressState)
                .MaximumLength(Address.StateMaxLength);
            
            RuleFor(x => x.PickupAddressZipCode)
                .NotEmpty()
                .MaximumLength(Address.ZipCodeMaxLength);
            
            RuleFor(x => x.PickupAddressCity)
                .NotEmpty()
                .MaximumLength(Address.CityMaxLength);
            
            RuleFor(x => x.PickupAddressStreet)
                .NotEmpty()
                .MaximumLength(Address.StreetMaxLength);
            
            RuleFor(x => x.PickupAddressBuildingNumber)
                .NotEmpty()
                .MaximumLength(Address.BuildingNumberMaxLength);
        }
    }

    internal sealed class UpdateAdvertisementCommandHandler(ApplicationDbContext db)
        : IRequestHandler<UpdateAdvertisementCommand>
    {
        public async Task Handle(UpdateAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement = 
                await db.Advertisements
                    .Where(x => x.Id == request.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            
            Address pickupAddress = new()
            {
                Country = request.PickupAddressCountry,
                State = request.PickupAddressState,
                ZipCode = request.PickupAddressZipCode,
                City = request.PickupAddressCity,
                Street = request.PickupAddressStreet,
                BuildingNumber = request.PickupAddressBuildingNumber
            };
            Email contactInfoEmail = Email.Create(request.ContactInfoEmail);
            PhoneNumber contactInfoPhoneNumber = PhoneNumber.Create(request.ContactInfoPhoneNumber);
            Description description = Description.Create(request.Description);
            
            advertisement.PickupAddress = pickupAddress;
            advertisement.ContactInfoEmail = contactInfoEmail;
            advertisement.ContactInfoPhoneNumber = contactInfoPhoneNumber;
            advertisement.Description = description;
            
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("advertisements/{id:guid}", async (
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
        Guid id);
}
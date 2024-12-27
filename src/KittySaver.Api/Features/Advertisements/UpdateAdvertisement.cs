using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
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
        Guid AdvertisementId,
        Guid PersonId,
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string? PickupAddressStreet,
        string? PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber) : IAdvertisementCommand;

    public sealed class UpdateAdvertisementCommandValidator
        : AbstractValidator<UpdateAdvertisementCommand>
    {
        public UpdateAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.AdvertisementId);
            
            RuleFor(x => x.AdvertisementId)
                .NotEmpty()
                .NotEqual(x => x.PersonId);
            
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
                .MaximumLength(Address.StreetMaxLength);
            
            RuleFor(x => x.PickupAddressBuildingNumber)
                .MaximumLength(Address.BuildingNumberMaxLength);
        }
    }

    internal sealed class UpdateAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateAdvertisementCommand>
    {
        public async Task Handle(UpdateAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);

            Address pickupAddress = Address.Create(
                country: request.PickupAddressCountry,
                state: request.PickupAddressState,
                zipCode: request.PickupAddressZipCode,
                city: request.PickupAddressCity,
                street: request.PickupAddressStreet,
                buildingNumber: request.PickupAddressBuildingNumber);
            Email contactInfoEmail = Email.Create(request.ContactInfoEmail);
            PhoneNumber contactInfoPhoneNumber = PhoneNumber.Create(request.ContactInfoPhoneNumber);
            Description description = Description.Create(request.Description);
            
            owner.UpdateAdvertisement(
                advertisementId: request.AdvertisementId,
                description: description,
                pickupAddress: pickupAddress,
                contactInfoEmail: contactInfoEmail,
                contactInfoPhoneNumber: contactInfoPhoneNumber);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}/advertisements/{advertisementId:guid}", async (
            Guid personId,
            Guid advertisementId,
            UpdateAdvertisementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UpdateAdvertisementCommand command = request.MapToUpdateAdvertisementCommand(personId, advertisementId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization()
        .WithName(EndpointNames.UpdateAdvertisement.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}

[Mapper]
public static partial class UpdateAdvertisementMapper
{
    public static partial UpdateAdvertisement.UpdateAdvertisementCommand MapToUpdateAdvertisementCommand(
        this UpdateAdvertisement.UpdateAdvertisementRequest request,
        Guid personId,
        Guid advertisementId);
}
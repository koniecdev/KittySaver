using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Requests;
using KittySaver.Shared.TypedIds;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Advertisements;

public sealed class UpdateAdvertisement : IEndpoint
{
    public sealed record UpdateAdvertisementCommand(
        AdvertisementId Id,
        PersonId PersonId,
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string? PickupAddressStreet,
        string? PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber) : ICommand<AdvertisementHateoasResponse>, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class UpdateAdvertisementCommandValidator
        : AbstractValidator<UpdateAdvertisementCommand>
    {
        public UpdateAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty();

            RuleFor(x => x.Id)
                .NotEmpty();
            
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
        : IRequestHandler<UpdateAdvertisementCommand, AdvertisementHateoasResponse>
    {
        public async Task<AdvertisementHateoasResponse> Handle(UpdateAdvertisementCommand request, CancellationToken cancellationToken)
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
                advertisementId: request.Id,
                description: description,
                pickupAddress: pickupAddress,
                contactInfoEmail: contactInfoEmail,
                contactInfoPhoneNumber: contactInfoPhoneNumber);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            AdvertisementStatus advertisementStatus = owner.Advertisements.First(x => x.Id == request.Id).Status;
            return new AdvertisementHateoasResponse(request.Id, request.PersonId, advertisementStatus);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}/advertisements/{id:guid}", async (
            Guid personId,
            Guid id,
            UpdateAdvertisementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UpdateAdvertisementCommand command = request.MapToUpdateAdvertisementCommand(personId, id);
            AdvertisementHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
            return Results.Ok(hateoasResponse);
        }).RequireAuthorization()
        .WithName(EndpointNames.UpdateAdvertisement.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}

[Mapper]
public static partial class UpdateAdvertisementMapper
{
    public static partial UpdateAdvertisement.UpdateAdvertisementCommand MapToUpdateAdvertisementCommand(
        this UpdateAdvertisementRequest request,
        Guid personId,
        Guid id);
}
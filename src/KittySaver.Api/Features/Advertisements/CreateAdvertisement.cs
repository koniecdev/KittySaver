using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Advertisements;

public class CreateAdvertisement : IEndpoint
{
    public sealed record CreateAdvertisementRequest(
        IEnumerable<Guid> CatsIdsToAssign,
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string? PickupAddressStreet,
        string? PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber
    );

    public sealed record CreateAdvertisementCommand(
        Guid PersonId,
        IEnumerable<Guid> CatsIdsToAssign,
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string? PickupAddressStreet,
        string? PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber) : IAdvertisementCommand<Guid>;

    public sealed class CreateAdvertisementCommandValidator
        : AbstractValidator<CreateAdvertisementCommand>
    {
        public CreateAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId).NotEmpty();

            RuleFor(x => x.CatsIdsToAssign).NotEmpty();

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

    internal sealed class CreateAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
        : IRequestHandler<CreateAdvertisementCommand, Guid>
    {
        public async Task<Guid> Handle(CreateAdvertisementCommand request, CancellationToken cancellationToken)
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
            
            Advertisement advertisement = owner.AddAdvertisement(
                dateTimeService.Now,
                request.CatsIdsToAssign,
                pickupAddress,
                contactInfoEmail,
                contactInfoPhoneNumber,
                description);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return advertisement.Id;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/advertisements", async (
            Guid personId,
            CreateAdvertisementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            CreateAdvertisementCommand command = request.MapToCreateAdvertisementCommand(personId);
            Guid advertisementId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/persons/{personId}/advertisements/{advertisementId}", new { Id = advertisementId });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class CreateAdvertisementMapper
{
    public static partial CreateAdvertisement.CreateAdvertisementCommand MapToCreateAdvertisementCommand(
        this CreateAdvertisement.CreateAdvertisementRequest request,
        Guid personId);
}
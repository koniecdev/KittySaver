using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Requests;
using KittySaver.Shared.TypedIds;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Advertisements;

public class CreateAdvertisement : IEndpoint
{
    public sealed record CreateAdvertisementCommand(
        PersonId PersonId,
        IEnumerable<CatId> CatsIdsToAssign,
        string? Description,
        string PickupAddressCountry,
        string? PickupAddressState,
        string PickupAddressZipCode,
        string PickupAddressCity,
        string? PickupAddressStreet,
        string? PickupAddressBuildingNumber,
        string ContactInfoEmail,
        string ContactInfoPhoneNumber) : ICommand<AdvertisementHateoasResponse>, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class CreateAdvertisementCommandValidator
    : AbstractValidator<CreateAdvertisementCommand>
{
    public CreateAdvertisementCommandValidator()
    {
        RuleFor(x => x.PersonId).NotEmpty()
            // .WithMessage("'Person Id' cannot be empty.");
            .WithMessage("'Id osoby' nie może być puste.");

        RuleFor(x => x.CatsIdsToAssign).NotEmpty()
            // .WithMessage("'Cats Ids To Assign' cannot be empty.");
            .WithMessage("'Identyfikatory kotów do przypisania' nie mogą być puste.");

        RuleFor(x => x.Description).MaximumLength(Description.MaxLength)
            // .WithMessage("'Description' must not exceed {MaxLength} characters.");
            .WithMessage("'Opis' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.ContactInfoPhoneNumber)
            .NotEmpty()
            // .WithMessage("'Contact Info Phone Number' cannot be empty.")
            .WithMessage("'Numer telefonu kontaktowego' nie może być pusty.")
            .MaximumLength(PhoneNumber.MaxLength)
            // .WithMessage("'Contact Info Phone Number' must not exceed {MaxLength} characters.");
            .WithMessage("'Numer telefonu kontaktowego' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.ContactInfoEmail)
            .NotEmpty()
            // .WithMessage("'Contact Info Email' cannot be empty.")
            .WithMessage("'Email kontaktowy' nie może być pusty.")
            .MaximumLength(Email.MaxLength)
            // .WithMessage("'Contact Info Email' must not exceed {MaxLength} characters.")
            .WithMessage("'Email kontaktowy' nie może przekraczać {MaxLength} znaków.")
            .Matches(Email.RegexPattern)
            // .WithMessage("'Contact Info Email' is not in the correct format.");
            .WithMessage("'Email kontaktowy' ma niepoprawny format.");

        RuleFor(x => x.PickupAddressCountry)
            .NotEmpty()
            // .WithMessage("'Pickup Address Country' cannot be empty.")
            .WithMessage("'Kraj w adresie odbioru' nie może być pusty.")
            .MaximumLength(Address.CountryMaxLength)
            // .WithMessage("'Pickup Address Country' must not exceed {MaxLength} characters.");
            .WithMessage("'Kraj w adresie odbioru' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.PickupAddressState)
            .MaximumLength(Address.StateMaxLength)
            // .WithMessage("'Pickup Address State' must not exceed {MaxLength} characters.");
            .WithMessage("'Województwo w adresie odbioru' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.PickupAddressZipCode)
            .NotEmpty()
            // .WithMessage("'Pickup Address Zip Code' cannot be empty.")
            .WithMessage("'Kod pocztowy w adresie odbioru' nie może być pusty.")
            .MaximumLength(Address.ZipCodeMaxLength)
            // .WithMessage("'Pickup Address Zip Code' must not exceed {MaxLength} characters.");
            .WithMessage("'Kod pocztowy w adresie odbioru' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.PickupAddressCity)
            .NotEmpty()
            // .WithMessage("'Pickup Address City' cannot be empty.")
            .WithMessage("'Miasto w adresie odbioru' nie może być puste.")
            .MaximumLength(Address.CityMaxLength)
            // .WithMessage("'Pickup Address City' must not exceed {MaxLength} characters.");
            .WithMessage("'Miasto w adresie odbioru' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.PickupAddressStreet)
            .MaximumLength(Address.StreetMaxLength)
            // .WithMessage("'Pickup Address Street' must not exceed {MaxLength} characters.");
            .WithMessage("'Ulica w adresie odbioru' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.PickupAddressBuildingNumber)
            .MaximumLength(Address.BuildingNumberMaxLength)
            // .WithMessage("'Pickup Address Building Number' must not exceed {MaxLength} characters.");
            .WithMessage("'Numer budynku w adresie odbioru' nie może przekraczać {MaxLength} znaków.");
    }
}

    internal sealed class CreateAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
        : IRequestHandler<CreateAdvertisementCommand, AdvertisementHateoasResponse>
    {
        public async Task<AdvertisementHateoasResponse> Handle(CreateAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetByIdAsync(request.PersonId, cancellationToken);

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
            return new AdvertisementHateoasResponse(
                advertisement.Id,
                advertisement.PersonId,
                advertisement.Status);
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
            AdvertisementHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/persons/{personId}/advertisements/{hateoasResponse.Id}", new
            {
                hateoasResponse.Id,
                hateoasResponse.PersonId,
                hateoasResponse.Status,
                hateoasResponse.Links
            });
        }).RequireAuthorization()
        .WithName(EndpointNames.Advertisements.Create.EndpointName)
        .WithTags(EndpointNames.Advertisements.Group);
    }
}

[Mapper]
public static partial class CreateAdvertisementMapper
{
    public static partial CreateAdvertisement.CreateAdvertisementCommand MapToCreateAdvertisementCommand(
        this CreateAdvertisementRequest request,
        Guid personId);
}
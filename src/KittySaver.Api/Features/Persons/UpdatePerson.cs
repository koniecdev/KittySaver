using FluentValidation;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Requests;
using KittySaver.Shared.TypedIds;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons;

public sealed class UpdatePerson : IEndpoint
{
    public sealed record UpdatePersonCommand(
        PersonId Id,
        string Nickname,
        string Email,
        string PhoneNumber,
        string DefaultAdvertisementPickupAddressCountry,
        string? DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string? DefaultAdvertisementPickupAddressStreet,
        string? DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber) : ICommand<PersonHateoasResponse>, IAuthorizedRequest, IPersonRequest;

    public sealed class UpdatePersonCommandValidator : AbstractValidator<UpdatePerson.UpdatePersonCommand>, IAsyncValidator
{
    public UpdatePersonCommandValidator(IPersonUniquenessChecksRepository personRepository)
    {
        RuleFor(x => x.Id).NotEmpty()
            // .WithMessage("'Id' cannot be empty.");
            .WithMessage("'Id' nie może być puste.");

        RuleFor(x => x.Nickname)
            .NotEmpty()
            // .WithMessage("'Nickname' cannot be empty.")
            .WithMessage("'Nazwa użytkownika' nie może być pusta.")
            .MaximumLength(Nickname.MaxLength)
            // .WithMessage("'Nickname' must not exceed {MaxLength} characters.");
            .WithMessage("'Nazwa użytkownika' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.Email)
            .NotEmpty()
            // .WithMessage("'Email' cannot be empty.")
            .WithMessage("'Email' nie może być pusty.")
            .MaximumLength(Email.MaxLength)
            // .WithMessage("'Email' must not exceed {MaxLength} characters.")
            .WithMessage("'Email' nie może przekraczać {MaxLength} znaków.")
            .Matches(Email.RegexPattern)
            // .WithMessage("'Email' is not in the correct format.")
            .WithMessage("'Email' ma niepoprawny format.")
            .MustAsync(async (command, email, ct) =>
                await personRepository.IsEmailUniqueAsync(email, command.Id, ct))
            // .WithMessage("'Email' is already used by another user.");
            .WithMessage("'Email' jest już używany przez innego użytkownika.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            // .WithMessage("'Phone Number' cannot be empty.")
            .WithMessage("'Numer telefonu' nie może być pusty.")
            .MaximumLength(PhoneNumber.MaxLength)
            // .WithMessage("'Phone Number' must not exceed {MaxLength} characters.")
            .WithMessage("'Numer telefonu' nie może przekraczać {MaxLength} znaków.")
            .MustAsync(async (command, phoneNumber, ct) =>
                await personRepository.IsPhoneNumberUniqueAsync(phoneNumber, command.Id, ct))
            // .WithMessage("'Phone Number' is already used by another user.");
            .WithMessage("'Numer telefonu' jest już używany przez innego użytkownika.");

        RuleFor(x => x.DefaultAdvertisementContactInfoPhoneNumber)
            .NotEmpty()
            // .WithMessage("'Default Advertisement Contact Info Phone Number' cannot be empty.")
            .WithMessage("'Domyślny numer telefonu kontaktowego dla ogłoszeń' nie może być pusty.")
            .MaximumLength(PhoneNumber.MaxLength)
            // .WithMessage("'Default Advertisement Contact Info Phone Number' must not exceed {MaxLength} characters.");
            .WithMessage("'Domyślny numer telefonu kontaktowego dla ogłoszeń' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.DefaultAdvertisementContactInfoEmail)
            .NotEmpty()
            // .WithMessage("'Default Advertisement Contact Info Email' cannot be empty.")
            .WithMessage("'Domyślny email kontaktowy dla ogłoszeń' nie może być pusty.")
            .MaximumLength(Email.MaxLength)
            // .WithMessage("'Default Advertisement Contact Info Email' must not exceed {MaxLength} characters.")
            .WithMessage("'Domyślny email kontaktowy dla ogłoszeń' nie może przekraczać {MaxLength} znaków.")
            .Matches(Email.RegexPattern)
            // .WithMessage("'Default Advertisement Contact Info Email' is not in the correct format.");
            .WithMessage("'Domyślny email kontaktowy dla ogłoszeń' ma niepoprawny format.");

        RuleFor(x => x.DefaultAdvertisementPickupAddressCountry)
            .NotEmpty()
            // .WithMessage("'Default Advertisement Pickup Address Country' cannot be empty.")
            .WithMessage("'Domyślny kraj w adresie odbioru dla ogłoszeń' nie może być pusty.")
            .MaximumLength(Address.CountryMaxLength)
            // .WithMessage("'Default Advertisement Pickup Address Country' must not exceed {MaxLength} characters.");
            .WithMessage("'Domyślny kraj w adresie odbioru dla ogłoszeń' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.DefaultAdvertisementPickupAddressState)
            .MaximumLength(Address.StateMaxLength)
            // .WithMessage("'Default Advertisement Pickup Address State' must not exceed {MaxLength} characters.");
            .WithMessage("'Domyślne województwo w adresie odbioru dla ogłoszeń' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.DefaultAdvertisementPickupAddressZipCode)
            .NotEmpty()
            // .WithMessage("'Default Advertisement Pickup Address Zip Code' cannot be empty.")
            .WithMessage("'Domyślny kod pocztowy w adresie odbioru dla ogłoszeń' nie może być pusty.")
            .MaximumLength(Address.ZipCodeMaxLength)
            // .WithMessage("'Default Advertisement Pickup Address Zip Code' must not exceed {MaxLength} characters.");
            .WithMessage("'Domyślny kod pocztowy w adresie odbioru dla ogłoszeń' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.DefaultAdvertisementPickupAddressCity)
            .NotEmpty()
            // .WithMessage("'Default Advertisement Pickup Address City' cannot be empty.")
            .WithMessage("'Domyślne miasto w adresie odbioru dla ogłoszeń' nie może być puste.")
            .MaximumLength(Address.CityMaxLength)
            // .WithMessage("'Default Advertisement Pickup Address City' must not exceed {MaxLength} characters.");
            .WithMessage("'Domyślne miasto w adresie odbioru dla ogłoszeń' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.DefaultAdvertisementPickupAddressStreet)
            .MaximumLength(Address.StreetMaxLength)
            // .WithMessage("'Default Advertisement Pickup Address Street' must not exceed {MaxLength} characters.");
            .WithMessage("'Domyślna ulica w adresie odbioru dla ogłoszeń' nie może przekraczać {MaxLength} znaków.");

        RuleFor(x => x.DefaultAdvertisementPickupAddressBuildingNumber)
            .MaximumLength(Address.BuildingNumberMaxLength)
            // .WithMessage("'Default Advertisement Pickup Address Building Number' must not exceed {MaxLength} characters.");
            .WithMessage("'Domyślny numer budynku w adresie odbioru dla ogłoszeń' nie może przekraczać {MaxLength} znaków.");
    }
}

    internal sealed class UpdatePersonCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpdatePersonCommand, PersonHateoasResponse>
    {
        public async Task<PersonHateoasResponse> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = await personRepository.GetPersonByIdAsync(request.Id, cancellationToken);

            Nickname nickname = Nickname.Create(request.Nickname);
            Email email = Email.Create(request.Email);
            PhoneNumber phoneNumber = PhoneNumber.Create(request.PhoneNumber);

            Address defaultAdvertisementPickupAddress = Address.Create(
                country: request.DefaultAdvertisementPickupAddressCountry,
                state: request.DefaultAdvertisementPickupAddressState,
                zipCode: request.DefaultAdvertisementPickupAddressZipCode,
                city: request.DefaultAdvertisementPickupAddressCity,
                street: request.DefaultAdvertisementPickupAddressStreet,
                buildingNumber: request.DefaultAdvertisementPickupAddressBuildingNumber);

            Email defaultAdvertisementContactInfoEmail = Email.Create(request.DefaultAdvertisementContactInfoEmail);
            PhoneNumber defaultAdvertisementContactInfoPhoneNumber =
                PhoneNumber.Create(request.DefaultAdvertisementContactInfoPhoneNumber);

            person.ChangeNickname(nickname);
            person.ChangeEmail(email);
            person.ChangePhoneNumber(phoneNumber);
            person.ChangeDefaultsForAdvertisement(
                defaultAdvertisementPickupAddress,
                defaultAdvertisementContactInfoEmail,
                defaultAdvertisementContactInfoPhoneNumber);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new PersonHateoasResponse(person.Id);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{id:guid}", async (
                Guid id,
                UpdatePersonRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                UpdatePersonCommand command = request.MapToUpdatePersonCommand(id);
                PersonHateoasResponse response = await sender.Send(command, cancellationToken);
                return Results.Ok(response);
            }).RequireAuthorization()
            .WithName(EndpointNames.UpdatePerson.EndpointName)
            .WithTags(EndpointNames.GroupNames.PersonGroup);
    }
}

[Mapper]
public static partial class UpdatePersonMapper
{
    public static partial UpdatePerson.UpdatePersonCommand MapToUpdatePersonCommand(
        this UpdatePersonRequest request,
        Guid id);
}
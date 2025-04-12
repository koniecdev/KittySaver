using FluentValidation;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Infrastructure.Clients;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Domain.Persons.ValueObjects;
using KittySaver.Domain.ValueObjects;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Requests;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons;

public sealed class CreatePerson : IEndpoint
{
    public sealed record CreatePersonCommand(
        string Nickname,
        string Email,
        string PhoneNumber,
        string Password,
        string DefaultAdvertisementPickupAddressCountry,
        string? DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string? DefaultAdvertisementPickupAddressStreet,
        string? DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber) : ICommand<PersonHateoasResponse>, ICreatePersonRequest;

    public sealed class CreatePersonCommandValidator : AbstractValidator<CreatePersonCommand>, IAsyncValidator
    {
        public CreatePersonCommandValidator(IPersonUniquenessChecksRepository personRepository)
        {
            RuleFor(x => x.Nickname)
                .NotEmpty()
                // .WithMessage("'Nickname' cannot be empty.")
                .WithMessage("'Nazwa użytkownika' nie może być pusta.")
                .MaximumLength(Nickname.MaxLength)
                // .WithMessage("'Nickname' must not exceed {MaxLength} characters.")
                .WithMessage("'Nazwa użytkownika' nie może przekraczać {MaxLength} znaków.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                // .WithMessage("'Password' is not in the correct format. Your password length must be at least 8.")
                .WithMessage("Twoje hasło musi mieć co najmniej 8 znaków.")
                .Matches("[A-Z]+")
                // .WithMessage("'Password' is not in the correct format. Your password must contain at least one uppercase letter.")
                .WithMessage("Twoje hasło musi zawierać co najmniej jedną wielką literę.")
                .Matches("[a-z]+")
                // .WithMessage("'Password' is not in the correct format. Your password must contain at least one lowercase letter.")
                .WithMessage("Twoje hasło musi zawierać co najmniej jedną małą literę.")
                .Matches("[0-9]+")
                // .WithMessage("'Password' is not in the correct format. Your password must contain at least one number.")
                .WithMessage("Twoje hasło musi zawierać co najmniej jedną cyfrę.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                // .WithMessage("'Phone Number' cannot be empty.")
                .WithMessage("'Numer telefonu' nie może być pusty.")
                .MaximumLength(PhoneNumber.MaxLength)
                // .WithMessage("'Phone Number' must not exceed {MaxLength} characters.")
                .WithMessage("'Numer telefonu' nie może przekraczać {MaxLength} znaków.")
                .MustAsync(async (phoneNumber, ct) =>
                    await personRepository.IsPhoneNumberUniqueAsync(phoneNumber, null, ct))
                // .WithMessage("'Phone Number' is already used by another user.");
                .WithMessage("'Numer telefonu' jest już używany przez innego użytkownika.");

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
                .MustAsync(async (email, ct) => await personRepository.IsEmailUniqueAsync(email, null, ct))
                // .WithMessage("'Email' is already used by another user.");
                .WithMessage("'Email' jest już używany przez innego użytkownika.");

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

    internal sealed class CreatePersonCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IAuthApiHttpClient authApiHttpClient)
        : IRequestHandler<CreatePersonCommand, PersonHateoasResponse>
    {
        public async Task<PersonHateoasResponse> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
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

            Person person = Person.Create(
                nickname: nickname,
                email: email,
                phoneNumber: phoneNumber,
                defaultAdvertisementPickupAddress: defaultAdvertisementPickupAddress,
                defaultAdvertisementContactInfoEmail: defaultAdvertisementContactInfoEmail,
                defaultAdvertisementContactInfoPhoneNumber: defaultAdvertisementContactInfoPhoneNumber);

            personRepository.Insert(person);
            
            RegisterRequest registerDto = new(
                UserName: person.Nickname,
                Email: person.Email,
                PhoneNumber: person.PhoneNumber,
                Password: request.Password);
        
            Guid userIdentityId = await authApiHttpClient.RegisterAsync<Guid>(registerDto, cancellationToken);

            person.SetUserIdentityId(userIdentityId);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return new PersonHateoasResponse(person.Id);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons", async (
                CreatePersonRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                CreatePersonCommand command = request.MapToCreatePersonCommand();
                PersonHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
                return Results.Created($"/api/v1/persons/{hateoasResponse.Id}",
                    new { id = hateoasResponse.Id, hateoasResponse.Links });
            }).AllowAnonymous()
            .WithName(EndpointNames.Persons.Create.EndpointName)
            .WithTags(EndpointNames.Persons.Group);
    }
}

[Mapper]
public static partial class CreatePersonMapper
{
    public static partial CreatePerson.CreatePersonCommand MapToCreatePersonCommand(
        this CreatePersonRequest request);
}
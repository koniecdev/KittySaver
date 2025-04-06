using FluentValidation;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
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
                .MaximumLength(Nickname.MaxLength);
            
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8).WithMessage("'Password' is not in the correct format. Your password length must be at least 8.")
                .Matches("[A-Z]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one uppercase letter.")
                .Matches("[a-z]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one lowercase letter.")
                .Matches("[0-9]+").WithMessage("'Password' is not in the correct format. Your password must contain at least one number.");
            
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(PhoneNumber.MaxLength)
                .MustAsync(async (phoneNumber, ct) => 
                    await personRepository.IsPhoneNumberUniqueAsync(phoneNumber, null, ct))
                .WithMessage("'Phone Number' is already used by another user.");
            
            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(Email.MaxLength)
                .Matches(Email.RegexPattern)
                .MustAsync(async (email, ct) => await personRepository.IsEmailUniqueAsync(email, null, ct))
                .WithMessage("'Email' is already used by another user.");
            
            RuleFor(x => x.DefaultAdvertisementContactInfoPhoneNumber)
                .NotEmpty()
                .MaximumLength(PhoneNumber.MaxLength);

            RuleFor(x => x.DefaultAdvertisementContactInfoEmail)
                .NotEmpty()
                .MaximumLength(Email.MaxLength)
                .Matches(Email.RegexPattern);
            
            RuleFor(x => x.DefaultAdvertisementPickupAddressCountry)
                .NotEmpty()
                .MaximumLength(Address.CountryMaxLength);
            
            RuleFor(x => x.DefaultAdvertisementPickupAddressState)
                .MaximumLength(Address.StateMaxLength);
            
            RuleFor(x => x.DefaultAdvertisementPickupAddressZipCode)
                .NotEmpty()
                .MaximumLength(Address.ZipCodeMaxLength);
            
            RuleFor(x => x.DefaultAdvertisementPickupAddressCity)
                .NotEmpty()
                .MaximumLength(Address.CityMaxLength);
            
            RuleFor(x => x.DefaultAdvertisementPickupAddressStreet)
                .MaximumLength(Address.StreetMaxLength);
            
            RuleFor(x => x.DefaultAdvertisementPickupAddressBuildingNumber)
                .MaximumLength(Address.BuildingNumberMaxLength);
        }
    }
    
    internal sealed class CreatePersonCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
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
            PhoneNumber defaultAdvertisementContactInfoPhoneNumber = PhoneNumber.Create(request.DefaultAdvertisementContactInfoPhoneNumber);
            
            Person person = Person.Create(
                nickname: nickname,
                email: email,
                phoneNumber: phoneNumber,
                defaultAdvertisementPickupAddress: defaultAdvertisementPickupAddress,
                defaultAdvertisementContactInfoEmail: defaultAdvertisementContactInfoEmail,
                defaultAdvertisementContactInfoPhoneNumber: defaultAdvertisementContactInfoPhoneNumber);
            
            await personRepository.InsertAsync(person, request.Password);
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
            return Results.Created($"/api/v1/persons/{hateoasResponse.Id}", new { id = hateoasResponse.Id, hateoasResponse.Links });
        }).AllowAnonymous()
        .WithName(EndpointNames.CreatePerson.EndpointName)
        .WithTags(EndpointNames.GroupNames.PersonGroup);
    }
}

[Mapper]
public static partial class CreatePersonMapper
{
    public static partial CreatePerson.CreatePersonCommand MapToCreatePersonCommand(
        this CreatePersonRequest request);
}
    

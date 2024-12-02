using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Api.Shared.Persistence.Repositories;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons;

public sealed class CreatePerson : IEndpoint
{
    public sealed record CreatePersonRequest(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        Guid UserIdentityId,
        string AddressCountry,
        string? AddressState,
        string AddressZipCode,
        string AddressCity,
        string AddressStreet,
        string AddressBuildingNumber,
        string DefaultAdvertisementPickupAddressCountry,
        string? DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string DefaultAdvertisementPickupAddressStreet,
        string DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber);
    
    public sealed record CreatePersonCommand(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        Guid UserIdentityId,
        string AddressCountry,
        string? AddressState,
        string AddressZipCode,
        string AddressCity,
        string AddressStreet,
        string AddressBuildingNumber,
        string DefaultAdvertisementPickupAddressCountry,
        string? DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string DefaultAdvertisementPickupAddressStreet,
        string DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber) : ICommand<Guid>;

    public sealed class CreatePersonCommandValidator 
        : AbstractValidator<CreatePersonCommand>, IAsyncValidator
    {
        public CreatePersonCommandValidator(IPersonRepository personRepository)
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(FirstName.MaxLength);
            
            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(LastName.MaxLength);
            
            RuleFor(x => x.UserIdentityId)
                .NotEmpty()
                .MustAsync(async (userIdentityId, ct) => 
                    await personRepository.IsUserIdentityIdUniqueAsync(userIdentityId, ct))
                .WithMessage("'User Identity Id' is already used by another user.");
            
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
            
            RuleFor(x => x.AddressCountry)
                .NotEmpty()
                .MaximumLength(Address.CountryMaxLength);
            
            RuleFor(x => x.AddressState)
                .MaximumLength(Address.StateMaxLength);
            
            RuleFor(x => x.AddressZipCode)
                .NotEmpty()
                .MaximumLength(Address.ZipCodeMaxLength);
            
            RuleFor(x => x.AddressCity)
                .NotEmpty()
                .MaximumLength(Address.CityMaxLength);
            
            RuleFor(x => x.AddressStreet)
                .NotEmpty()
                .MaximumLength(Address.StreetMaxLength);
            
            RuleFor(x => x.AddressBuildingNumber)
                .NotEmpty()
                .MaximumLength(Address.BuildingNumberMaxLength);
            
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
    
    internal sealed class CreatePersonCommandHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork)
        : IRequestHandler<CreatePersonCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            FirstName firstName = FirstName.Create(request.FirstName);
            LastName lastName = LastName.Create(request.LastName);
            Email email = Email.Create(request.Email);
            PhoneNumber phoneNumber = PhoneNumber.Create(request.PhoneNumber);
            
            Address residentalAddress = Address.Create(
                country: request.AddressCountry,
                state: request.AddressState,
                zipCode: request.AddressZipCode,
                city: request.AddressCity,
                street: request.AddressStreet,
                buildingNumber: request.AddressBuildingNumber);
            
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
                userIdentityId: request.UserIdentityId,
                firstName: firstName,
                lastName: lastName,
                email: email,
                phoneNumber: phoneNumber,
                residentalAddress: residentalAddress,
                defaultAdvertisementPickupAddress: defaultAdvertisementPickupAddress,
                defaultAdvertisementContactInfoEmail: defaultAdvertisementContactInfoEmail,
                defaultAdvertisementContactInfoPhoneNumber: defaultAdvertisementContactInfoPhoneNumber);
            
            personRepository.Insert(person);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return person.Id;
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
            Guid personId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/persons/{personId}", new { Id = personId });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class CreatePersonMapper
{
    public static partial CreatePerson.CreatePersonCommand MapToCreatePersonCommand(
        this CreatePerson.CreatePersonRequest request);
}
    

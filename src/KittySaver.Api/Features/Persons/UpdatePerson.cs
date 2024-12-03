using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons;

public sealed class UpdatePerson : IEndpoint
{
    public sealed record UpdatePersonRequest(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
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

    public sealed record UpdatePersonCommand(
        Guid IdOrUserIdentityId,
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
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
        string DefaultAdvertisementContactInfoPhoneNumber) : ICommand;

    public sealed class UpdatePersonCommandValidator
        : AbstractValidator<UpdatePersonCommand>, IAsyncValidator
    {
        public UpdatePersonCommandValidator(IPersonRepository personRepository)
        {
            RuleFor(x => x.IdOrUserIdentityId).NotEmpty();

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(FirstName.MaxLength);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(LastName.MaxLength);

            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(Email.MaxLength)
                .Matches(Email.RegexPattern)
                .MustAsync(async (command, email, ct) => 
                    await personRepository.IsEmailUniqueAsync(email, command.IdOrUserIdentityId, ct))
                .WithMessage("'Email' is already used by another user.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(PhoneNumber.MaxLength)
                .MustAsync(async (command, phoneNumber, ct) => 
                    await personRepository.IsPhoneNumberUniqueAsync(phoneNumber, command.IdOrUserIdentityId, ct))
                .WithMessage("'Phone Number' is already used by another user.");

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

    internal sealed class UpdatePersonCommandHandler(IPersonRepository personRepository, IUnitOfWork unitOfWork)
        : IRequestHandler<UpdatePersonCommand>
    {
        public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = await personRepository.GetPersonByIdOrIdentityIdAsync(request.IdOrUserIdentityId, cancellationToken);

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

            person.ChangeName(firstName, lastName);
            person.ChangeEmail(email);
            person.ChangePhoneNumber(phoneNumber);
            person.ChangeResidentalAddress(residentalAddress);
            person.ChangeDefaultsForAdvertisement(
                defaultAdvertisementPickupAddress,
                defaultAdvertisementContactInfoEmail,
                defaultAdvertisementContactInfoPhoneNumber);

            await unitOfWork.SaveChangesAsync(cancellationToken);
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
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        });
    }
}

[Mapper]
public static partial class UpdatePersonMapper
{
    public static partial UpdatePerson.UpdatePersonCommand MapToUpdatePersonCommand(
        this UpdatePerson.UpdatePersonRequest request,
        Guid idOrUserIdentityId);
}
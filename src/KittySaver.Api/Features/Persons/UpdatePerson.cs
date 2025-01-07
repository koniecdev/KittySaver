using FluentValidation;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Contracts;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons;

public sealed class UpdatePerson : IEndpoint
{
    public sealed record UpdatePersonRequest(
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
        string DefaultAdvertisementContactInfoPhoneNumber);

    public sealed record UpdatePersonCommand(
        Guid IdOrUserIdentityId,
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

    public sealed class UpdatePersonCommandValidator : AbstractValidator<UpdatePersonCommand>, IAsyncValidator
    {
        public UpdatePersonCommandValidator(IPersonUniquenessChecksRepository personRepository)
        {
            RuleFor(x => x.IdOrUserIdentityId).NotEmpty();

            RuleFor(x => x.Nickname)
                .NotEmpty()
                .MaximumLength(Nickname.MaxLength);

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

    internal sealed class UpdatePersonCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpdatePersonCommand, PersonHateoasResponse>
    {
        public async Task<PersonHateoasResponse> Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = await personRepository.GetPersonByIdOrIdentityIdAsync(request.IdOrUserIdentityId, cancellationToken);

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
        this UpdatePerson.UpdatePersonRequest request,
        Guid idOrUserIdentityId);
}
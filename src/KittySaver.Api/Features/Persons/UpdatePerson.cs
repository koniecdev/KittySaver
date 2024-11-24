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
        private readonly ApplicationDbContext _db;

        public UpdatePersonCommandValidator(ApplicationDbContext db)
        {
            _db = db;
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
                .MustAsync(async (command, email, ct) => await IsEmailUniqueAsync(command, email, ct))
                .WithMessage("'Email' is already used by another user.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(PhoneNumber.MaxLength)
                .MustAsync(async (command, phoneNumber, ct) => await IsPhoneNumberUniqueAsync(command, phoneNumber, ct))
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

        private async Task<bool> IsPhoneNumberUniqueAsync(UpdatePersonCommand command, string phone,
            CancellationToken ct)
            => !await _db.Persons
                .AsNoTracking()
                .AnyAsync(
                    x => x.PhoneNumber.Value == phone && x.Id != command.IdOrUserIdentityId &&
                         x.UserIdentityId != command.IdOrUserIdentityId, ct);

        private async Task<bool> IsEmailUniqueAsync(UpdatePersonCommand command, string email, CancellationToken ct)
            => !await _db.Persons
                .AsNoTracking()
                .AnyAsync(
                    x => x.Email.Value == email && x.Id != command.IdOrUserIdentityId &&
                         x.UserIdentityId != command.IdOrUserIdentityId, ct);
    }

    internal sealed class UpdatePersonCommandHandler(ApplicationDbContext db)
        : IRequestHandler<UpdatePersonCommand>
    {
        public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person =
                await db.Persons
                    .Where(x =>
                        x.Id == request.IdOrUserIdentityId
                        || x.UserIdentityId == request.IdOrUserIdentityId)
                    .Include(x => x.Cats)
                    .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundExceptions.PersonNotFoundException(request.IdOrUserIdentityId);

            FirstName firstName = FirstName.Create(request.FirstName);
            LastName lastName = LastName.Create(request.LastName);
            Email email = Email.Create(request.Email);
            PhoneNumber phoneNumber = PhoneNumber.Create(request.PhoneNumber);
            Address residentalAddress = new()
            {
                Country = request.AddressCountry,
                State = request.AddressState,
                ZipCode = request.AddressZipCode,
                City = request.AddressCity,
                Street = request.AddressStreet,
                BuildingNumber = request.AddressBuildingNumber
            };
            Address defaultAdvertisementPickupAddress = new()
            {
                Country = request.DefaultAdvertisementPickupAddressCountry,
                State = request.DefaultAdvertisementPickupAddressState,
                ZipCode = request.DefaultAdvertisementPickupAddressZipCode,
                City = request.DefaultAdvertisementPickupAddressCity,
                Street = request.DefaultAdvertisementPickupAddressStreet,
                BuildingNumber = request.DefaultAdvertisementPickupAddressBuildingNumber
            };
            Email defaultAdvertisementContactInfoEmail = Email.Create(request.DefaultAdvertisementContactInfoEmail);
            PhoneNumber defaultAdvertisementContactInfoPhoneNumber =
                PhoneNumber.Create(request.DefaultAdvertisementContactInfoPhoneNumber);

            person.FirstName = firstName;
            person.LastName = lastName;
            person.Email = email;
            person.PhoneNumber = phoneNumber;
            person.ResidentalAddress = residentalAddress;
            person.DefaultAdvertisementsPickupAddress = defaultAdvertisementPickupAddress;
            person.DefaultAdvertisementsContactInfoEmail = defaultAdvertisementContactInfoEmail;
            person.DefaultAdvertisementsContactInfoPhoneNumber = defaultAdvertisementContactInfoPhoneNumber;

            await db.SaveChangesAsync(cancellationToken);
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
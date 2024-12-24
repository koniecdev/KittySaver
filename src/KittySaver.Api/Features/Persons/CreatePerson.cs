using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons;

public sealed class CreatePerson : IEndpoint
{
    public sealed record CreatePersonRequest(
        string Nickname,
        string Email,
        string PhoneNumber,
        Guid UserIdentityId,
        string DefaultAdvertisementPickupAddressCountry,
        string? DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string? DefaultAdvertisementPickupAddressStreet,
        string? DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber);
    
    public sealed record IPersonCommand(
        string Nickname,
        string Email,
        string PhoneNumber,
        Guid UserIdentityId,
        string DefaultAdvertisementPickupAddressCountry,
        string? DefaultAdvertisementPickupAddressState,
        string DefaultAdvertisementPickupAddressZipCode,
        string DefaultAdvertisementPickupAddressCity,
        string? DefaultAdvertisementPickupAddressStreet,
        string? DefaultAdvertisementPickupAddressBuildingNumber,
        string DefaultAdvertisementContactInfoEmail,
        string DefaultAdvertisementContactInfoPhoneNumber) : IPersonCommand<Guid>;

    public sealed class CreatePersonCommandValidator 
        : AbstractValidator<IPersonCommand>, IAsyncValidator
    {
        public CreatePersonCommandValidator(IPersonRepository personRepository)
        {
            RuleFor(x => x.Nickname)
                .NotEmpty()
                .MaximumLength(Nickname.MaxLength);
            
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
        : IRequestHandler<IPersonCommand, Guid>
    {
        public async Task<Guid> Handle(IPersonCommand request, CancellationToken cancellationToken)
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
                userIdentityId: request.UserIdentityId,
                nickname: nickname,
                email: email,
                phoneNumber: phoneNumber,
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
            IPersonCommand command = request.MapToCreatePersonCommand();
            Guid personId = await sender.Send(command, cancellationToken);
            return Results.Created($"/api/v1/persons/{personId}", new { Id = personId });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class CreatePersonMapper
{
    public static partial CreatePerson.IPersonCommand MapToCreatePersonCommand(
        this CreatePerson.CreatePersonRequest request);
}
    

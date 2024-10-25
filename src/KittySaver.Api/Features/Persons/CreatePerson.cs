using FluentValidation;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.ValueObjects;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons;

public class CreatePerson : IEndpoint
{
    public sealed record CreatePersonRequest(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        Guid UserIdentityId,
        string AddressCountry,
        string AddressZipCode,
        string AddressCity,
        string AddressStreet,
        string AddressBuildingNumber,
        string? AddressState = null);
    
    public sealed record CreatePersonCommand(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        Guid UserIdentityId,
        string AddressCountry,
        string AddressZipCode,
        string AddressCity,
        string AddressStreet,
        string AddressBuildingNumber,
        string? AddressState = null) : ICommand<Guid>;

    public sealed class CreatePersonCommandValidator 
        : AbstractValidator<CreatePersonCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;

        public CreatePersonCommandValidator(ApplicationDbContext db)
        {
            _db = db;
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(Person.Constraints.FirstNameMaxLength);
            
            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(Person.Constraints.LastNameMaxLength);
            
            RuleFor(x => x.PhoneNumber)
                .NotEmpty()
                .MaximumLength(Person.Constraints.PhoneNumberMaxLength)
                .MustAsync(async (phoneNumber, ct) => await IsPhoneNumberUniqueAsync(phoneNumber, ct))
                .WithMessage("'Phone Number' is already used by another user.");
            
            RuleFor(x => x.UserIdentityId)
                .NotEmpty()
                .MustAsync(async (userIdentityId, ct) => await IsUserIdentityIdUniqueAsync(userIdentityId, ct))
                .WithMessage("'User Identity Id' is already used by another user.");
            
            RuleFor(x => x.Email)
                .NotEmpty()
                .MaximumLength(Person.Constraints.EmailMaxLength)
                .Matches(Person.Constraints.EmailPattern)
                .MustAsync(async (email, ct) => await IsEmailUniqueAsync(email, ct))
                .WithMessage("'Email' is already used by another user.");
            
            RuleFor(x => x.AddressCountry)
                .NotEmpty()
                .MaximumLength(Address.Constraints.CountryMaxLength);
            
            RuleFor(x => x.AddressState)
                .MaximumLength(Address.Constraints.StateMaxLength);
            
            RuleFor(x => x.AddressZipCode)
                .NotEmpty()
                .MaximumLength(Address.Constraints.ZipCodeMaxLength);
            
            RuleFor(x => x.AddressCity)
                .NotEmpty()
                .MaximumLength(Address.Constraints.CityMaxLength);
            
            RuleFor(x => x.AddressStreet)
                .NotEmpty()
                .MaximumLength(Address.Constraints.StreetMaxLength);
            
            RuleFor(x => x.AddressBuildingNumber)
                .NotEmpty()
                .MaximumLength(Address.Constraints.BuildingNumberMaxLength);
        }
        private async Task<bool> IsPhoneNumberUniqueAsync(string phone, CancellationToken ct) 
            => !await _db.Persons
                .AsNoTracking()
                .AnyAsync(x=>x.PhoneNumber == phone, ct);
        
        private async Task<bool> IsEmailUniqueAsync(string email, CancellationToken ct) 
            => !await _db.Persons
                .AsNoTracking()
                .AnyAsync(x=>x.Email == email, ct);

        private async Task<bool> IsUserIdentityIdUniqueAsync(Guid userIdentityId, CancellationToken ct) 
            => !await _db.Persons
                .AsNoTracking()
                .AnyAsync(x=>x.UserIdentityId == userIdentityId, ct);
    }
    
    internal sealed class CreatePersonCommandHandler(ApplicationDbContext db) : IRequestHandler<CreatePersonCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            Address address = new()
            {
                Country = request.AddressCountry,
                State = request.AddressState,
                ZipCode = request.AddressZipCode,
                City = request.AddressCity,
                Street = request.AddressStreet,
                BuildingNumber = request.AddressBuildingNumber
            };
            
            Person person = request.MapToEntity(address);
            db.Persons.Add(person);
            await db.SaveChangesAsync(cancellationToken);
            return person.Id;
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons", async 
            (CreatePersonRequest request,
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
    [UserMapping(Default = true)]
    public static CreatePerson.CreatePersonCommand MapToCreatePersonCommand(this CreatePerson.CreatePersonRequest request)
    {
        if (request.AddressState is not null && string.IsNullOrWhiteSpace(request.AddressState))
        {
            request = request with { AddressState = null };
        }
        CreatePerson.CreatePersonCommand dto = ToCreatePersonCommand(request);
        return dto;
    }
    private static partial CreatePerson.CreatePersonCommand ToCreatePersonCommand(this CreatePerson.CreatePersonRequest request);
    
    public static partial Person MapToEntity(this CreatePerson.CreatePersonCommand command, Address address);
}

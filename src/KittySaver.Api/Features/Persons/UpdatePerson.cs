using FluentValidation;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Domain.ValueObjects;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Persons;

public sealed class UpdatePerson : IEndpoint
{
    public sealed record UpdatePersonRequest(
        string FirstName,
        string LastName,
        string AddressCountry,
        string AddressZipCode,
        string AddressCity,
        string AddressStreet,
        string AddressBuildingNumber,
        string? AddressState = null);

    public sealed record UpdatePersonCommand(
        Guid IdOrUserIdentityId,
        string FirstName,
        string LastName,
        string AddressCountry,
        string AddressZipCode,
        string AddressCity,
        string AddressStreet,
        string AddressBuildingNumber,
        string? AddressState = null) : ICommand;

    public sealed class UpdatePersonCommandValidator
        : AbstractValidator<UpdatePersonCommand>
    {
        public UpdatePersonCommandValidator()
        {
            RuleFor(x => x.IdOrUserIdentityId).NotEmpty();
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            
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
    }

    internal sealed class UpdatePersonCommandHandler(ApplicationDbContext db)
        : IRequestHandler<UpdatePersonCommand>
    {
        public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = await db.Persons
                                .Where(x => 
                                    x.Id == request.IdOrUserIdentityId
                                    || x.UserIdentityId == request.IdOrUserIdentityId)
                                .Include(x => x.Cats)
                                .FirstOrDefaultAsync(cancellationToken)
                            ?? throw new NotFoundExceptions.PersonNotFoundException(request.IdOrUserIdentityId);
            
            Address address = new()
            {
                Country = request.AddressCountry,
                State = request.AddressState,
                ZipCode = request.AddressZipCode,
                City = request.AddressCity,
                Street = request.AddressStreet,
                BuildingNumber = request.AddressBuildingNumber
            };
            
            person.FirstName = request.FirstName;
            person.LastName = request.LastName;
            person.Address = address;
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
            UpdatePersonCommand command = request.ToUpdatePersonCommand(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        });
    }
}

[Mapper]
public static partial class UpdatePersonMapper
{
    public static partial UpdatePerson.UpdatePersonCommand ToUpdatePersonCommand(
        this UpdatePerson.UpdatePersonRequest request,
        Guid idOrUserIdentityId);
}
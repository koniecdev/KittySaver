using FluentValidation;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Domain.Entites;
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
        string Email,
        string PhoneNumber);
    
    public sealed record UpdatePersonCommand(
        Guid Id,
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber) : ICommand;

    public sealed class UpdatePersonCommandValidator
        : AbstractValidator<UpdatePersonCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;
        public UpdatePersonCommandValidator(ApplicationDbContext db)
        {
            _db = db; 
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.PhoneNumber)
                .MustAsync(async (command, email, ct) => await IsPhoneUniqueAsync(command, email, ct))
                .WithMessage("'Phone Number' is already used by another user.");
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern);
            RuleFor(x => x.Email)
                .MustAsync(async (command, email, ct) => await IsEmailUniqueAsync(command, email, ct))
                .WithMessage("'Email' is already used by another user.");
        }
        private async Task<bool> IsEmailUniqueAsync(UpdatePersonCommand command, string email, CancellationToken ct)
            =>!await _db.Persons
                .AnyAsync(x=>x.Email == email && x.Id != command.Id, ct);
        
        private async Task<bool> IsPhoneUniqueAsync(UpdatePersonCommand command, string phone, CancellationToken ct)
            => !await _db.Persons
                .AnyAsync(x=>x.PhoneNumber == phone && x.Id != command.Id, ct);
    }
    
    internal sealed class UpdatePersonCommandHandler(ApplicationDbContext db)
        : IRequestHandler<UpdatePersonCommand>
    {
        public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            int numberOfUpdatedPersons = await db.Persons
                .Where(x => x.Id == request.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x=>x.FirstName, request.FirstName)
                    .SetProperty(x=>x.LastName, request.LastName)
                    .SetProperty(x=>x.Email, request.Email)
                    .SetProperty(x=>x.PhoneNumber, request.PhoneNumber),
                    cancellationToken);
            if (numberOfUpdatedPersons == 0)
            {
                throw new Person.PersonNotFoundException(request.Id);
            }
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{id:guid}", async
            (Guid id, 
            UpdatePersonRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UpdatePersonCommand command = request.ToUpdateCommand(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        });
    }
}

[Mapper]
public static partial class UpdatePersonMapper
{
    public static partial UpdatePerson.UpdatePersonCommand ToUpdateCommand(this UpdatePerson.UpdatePersonRequest request, Guid id);
}

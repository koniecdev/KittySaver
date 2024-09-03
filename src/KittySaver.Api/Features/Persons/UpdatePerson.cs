using FluentValidation;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Endpoints;
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
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => await IsUserExistingInDatabase(id, ct))
                .WithMessage("User must exist in database to delete it");
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern);
            RuleFor(x => x.Email)
                .MustAsync(async (command, email, ct) => !await IsEmailAlreadyUsedBySomeoneElseInDb(command, email, ct))
                .WithMessage("Email is already used by different account in database");
        }

        private async Task<bool> IsEmailAlreadyUsedBySomeoneElseInDb(UpdatePersonCommand command, string email, CancellationToken ct)
        {
            return await _db.Persons.AnyAsync(x => x.Email == email && x.Id != command.Id, ct);
        } 
        private async Task<bool> IsUserExistingInDatabase(Guid id, CancellationToken ct) 
            => await _db.Persons.AnyAsync(x=>x.Id == id, ct);
    }
    
    internal sealed class UpdatePersonCommandHandler(ApplicationDbContext db)
        : IRequestHandler<UpdatePersonCommand>
    {
        public async Task Handle(UpdatePersonCommand request, CancellationToken cancellationToken)
        {
            Person user = await db.Persons
                .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new Person.Exceptions.PersonNotFoundException();
            UpdatePersonMapper.UpdateEntityWithCommandData(request, user);
            await db.SaveChangesAsync(cancellationToken);
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
    public static partial void UpdateEntityWithCommandData(UpdatePerson.UpdatePersonCommand source,
        Person target);
}

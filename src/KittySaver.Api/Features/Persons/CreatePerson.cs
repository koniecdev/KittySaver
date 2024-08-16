using FluentValidation;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Endpoints;
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
        Guid UserIdentityId);
    
    public sealed record CreatePersonCommand(
        string FirstName,
        string LastName,
        string Email,
        string PhoneNumber,
        Guid UserIdentityId) : ICommand<Guid>;

    public sealed class CreatePersonCommandValidator 
        : AbstractValidator<CreatePersonCommand>, IAsyncValidator
    {
        private readonly ApplicationDbContext _db;
        private const string EmailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

        public CreatePersonCommandValidator(ApplicationDbContext db)
        {
            _db = db;
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.UserIdentityId).NotEmpty();
            RuleFor(x => x.Email)
                .Matches(EmailPattern)
                .NotEmpty();
            RuleFor(x => x.Email)
                .MustAsync(async (email, ct) => await IsEmailNotAlreadyRegisteredInDb(email, ct))
                .WithMessage("Email is already registered in database");
            RuleFor(x => x.UserIdentityId)
                .MustAsync(async (userIdentityId, ct) => await IsUserIdentityIdNotAlreadyRegisteredInDb(userIdentityId, ct))
                .WithMessage("UserIdentityId is already registered in database");
        }

        private async Task<bool> IsEmailNotAlreadyRegisteredInDb(string email, CancellationToken ct)
        {
            return await _db.Persons.AnyAsync(x=>x.Email != email, ct);
        }
        
        private async Task<bool> IsUserIdentityIdNotAlreadyRegisteredInDb(Guid userIdentityId, CancellationToken ct)
        {
            return await _db.Persons.AnyAsync(x=>x.UserIdentityId != userIdentityId, ct);
        }
    }
    
    internal sealed class CreatePersonCommandHandler(ApplicationDbContext db) : IRequestHandler<CreatePersonCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = request.ToEntity();
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
            CreatePersonCommand command = request.ToCreatePersonCommand();
            Guid personId = await sender.Send(command, cancellationToken);
            return Results.Created($"/persons/{personId}", new { Id = personId });
        });
    }
}

[Mapper]
public static partial class CreatePersonMapper
{
    public static partial CreatePerson.CreatePersonCommand ToCreatePersonCommand(this CreatePerson.CreatePersonRequest request);
    public static partial Person ToEntity(this CreatePerson.CreatePersonCommand command);
}

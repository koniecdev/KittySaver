using FluentValidation;
using KittySaver.Api.Features.Persons.SharedContracts;
using KittySaver.Api.Shared.Domain.Entites;
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

        public CreatePersonCommandValidator(ApplicationDbContext db)
        {
            _db = db;
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.PhoneNumber)
                .MustAsync(async (phoneNumber, ct) => await IsPhoneNumberUniqueAsync(phoneNumber, ct))
                .WithMessage("'Phone Number' is already used by another user.");
            
            RuleFor(x => x.UserIdentityId).NotEmpty();
            RuleFor(x => x.UserIdentityId)
                .MustAsync(async (userIdentityId, ct) => await IsUserIdentityIdUniqueAsync(userIdentityId, ct))
                .WithMessage("'User Identity Id' is already used by another user.");
            
            RuleFor(x => x.Email)
                .NotEmpty()
                .Matches(ValidationPatterns.EmailPattern);
            RuleFor(x => x.Email)
                .MustAsync(async (email, ct) => await IsEmailUniqueAsync(email, ct))
                .WithMessage("'Email' is already used by another user.");
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
            return Results.Created($"/api/v1/persons/{personId}", new { Id = personId });
        }).RequireAuthorization();
    }
}

[Mapper]
public static partial class CreatePersonMapper
{
    public static partial CreatePerson.CreatePersonCommand ToCreatePersonCommand(this CreatePerson.CreatePersonRequest request);
    public static partial Person ToEntity(this CreatePerson.CreatePersonCommand command);
}

using FluentValidation;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Endpoints;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        string Password,
        Guid UserIdentityId) : ICommand<Guid>;

    public sealed class CreatePersonCommandValidator 
        : AbstractValidator<CreatePersonCommand>
    {
        private const string EmailPattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";

        public CreatePersonCommandValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.PhoneNumber).NotEmpty();
            RuleFor(x => x.UserIdentityId).NotEmpty();
            RuleFor(x => x.Email)
                .Matches(EmailPattern)
                .NotEmpty();
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

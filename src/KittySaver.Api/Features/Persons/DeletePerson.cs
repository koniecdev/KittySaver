using FluentValidation;
using KittySaver.Api.Infrastructure.Clients;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Features.Persons;

public sealed class DeletePerson : IEndpoint
{
    public sealed record DeletePersonCommand(PersonId Id) : ICommand, IAuthorizedRequest, IPersonRequest;

    public sealed class DeletePersonCommandValidator : AbstractValidator<DeletePersonCommand>
    {
        public DeletePersonCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty()
                // .WithMessage("'Id' cannot be empty.");
                .WithMessage("'Id' nie może być puste.");
        }
    }

    internal sealed class DeletePersonCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IAuthApiHttpClient authApiHttpClient)
        : IRequestHandler<DeletePersonCommand>
    {
        public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = await personRepository.GetPersonByIdAsync(request.Id, cancellationToken);
            
            personRepository.Remove(person);
            
            await authApiHttpClient.DeletePersonAsync(person.UserIdentityId);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("persons/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeletePersonCommand command = new(new PersonId(id));
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization()
        .WithName(EndpointNames.Persons.Delete.EndpointName)
        .WithTags(EndpointNames.Persons.Group);
    }
}
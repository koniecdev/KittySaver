using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Abstractions.Clients;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using MediatR;

namespace KittySaver.Api.Features.Persons;

public sealed class DeletePerson : IEndpoint
{
    public sealed record DeletePersonCommand(Guid Id, string AuthHeader) : ICommand, IAuthorizedRequest, IPersonRequest;

    public sealed class DeletePersonCommandValidator : AbstractValidator<DeletePersonCommand>
    {
        public DeletePersonCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class DeletePersonCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<DeletePersonCommand>
    {
        public async Task Handle(DeletePersonCommand request, CancellationToken cancellationToken)
        {
            Person person = await personRepository.GetPersonByIdAsync(request.Id, cancellationToken);
            
            await personRepository.RemoveAsync(person, request.AuthHeader);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("persons/{id:guid}", async (
            Guid id,
            ISender sender,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            string authHeader = httpContext.Request.Headers.Authorization.ToString();
            DeletePersonCommand command = new(id, authHeader);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization()
        .WithName(EndpointNames.DeletePerson.EndpointName)
        .WithTags(EndpointNames.GroupNames.PersonGroup);
    }
}
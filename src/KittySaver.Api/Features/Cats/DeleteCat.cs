using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Features.Cats;

public sealed class DeleteCat : IEndpoint
{
    public sealed record DeleteCatCommand(PersonId PersonId, CatId Id) : ICommand, IAuthorizedRequest, ICatRequest;

    public sealed class DeleteCatCommandValidator : AbstractValidator<DeleteCatCommand>
    {
        public DeleteCatCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.PersonId)
                .NotEmpty();
        }
    }

    internal sealed class DeleteCatCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteCatCommand>
    {
        public async Task Handle(DeleteCatCommand request, CancellationToken cancellationToken)
        {
            Person catOwner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            catOwner.RemoveCat(request.Id);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("/persons/{personId:guid}/cats/{id:guid}", async (
            Guid personId,
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteCatCommand command = new(new PersonId(personId), new CatId(id));
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization()
        .WithName(EndpointNames.DeleteCat.EndpointName)
        .WithTags(EndpointNames.GroupNames.CatGroup);
    }
}
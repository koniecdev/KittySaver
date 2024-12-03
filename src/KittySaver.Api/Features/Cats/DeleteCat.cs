using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class DeleteCat : IEndpoint
{
    public sealed record DeleteCatCommand(Guid PersonId, Guid Id) : ICommand;

    public sealed class DeleteCatCommandValidator
        : AbstractValidator<DeleteCatCommand>
    {
        public DeleteCatCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .NotEqual(x => x.PersonId);
            
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.Id);
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
            DeleteCatCommand command = new(personId, id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
using FluentValidation;
using KittySaver.Api.Shared.Domain.Entites;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class DeleteCat : IEndpoint
{
    public sealed record DeleteCatCommand(Guid Id) : ICommand;

    public sealed class DeleteCatCommandValidator
        : AbstractValidator<DeleteCatCommand>
    {
        public DeleteCatCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class DeleteCatCommandHandler(ApplicationDbContext db)
        : IRequestHandler<DeleteCatCommand>
    {
        public async Task Handle(DeleteCatCommand request, CancellationToken cancellationToken)
        {
            int numberOfDeletedCats = await db.Cats
                .Where(x => 
                    x.Id == request.Id)
                .ExecuteDeleteAsync(cancellationToken);
            if (numberOfDeletedCats == 0)
            {
                throw new Cat.CatNotFoundException(request.Id);
            }
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("cats/{id:guid}", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteCatCommand command = new(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
using FluentValidation;
using KittySaver.Api.Shared.Domain.Advertisements;
using KittySaver.Api.Shared.Domain.Persons;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class DeleteAdvertisement : IEndpoint
{
    public sealed record DeleteAdvertisementCommand(Guid Id) : ICommand;

    public sealed class DeleteAdvertisementCommandValidator
        : AbstractValidator<DeleteAdvertisementCommand>
    {
        public DeleteAdvertisementCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class DeleteAdvertisementCommandHandler(ApplicationDbContext db)
        : IRequestHandler<DeleteAdvertisementCommand>
    {
        public async Task Handle(DeleteAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement = 
                await db.Advertisements
                    .FirstOrDefaultAsync(x=> x.Id == request.Id, cancellationToken)
                    ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            db.Remove(advertisement);
            advertisement.AnnounceDeletion();
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("advertisements/{id:guid}", async (Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteAdvertisementCommand command = new(id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
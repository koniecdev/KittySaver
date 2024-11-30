using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class CloseAdvertisement : IEndpoint
{
    public sealed record CloseAdvertisementCommand(Guid Id) : ICommand;

    public sealed class CloseAdvertisementCommandValidator
        : AbstractValidator<CloseAdvertisementCommand>
    {
        public CloseAdvertisementCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class CloseAdvertisementCommandHandler(ApplicationDbContext db, IDateTimeService dateTimeService)
        : IRequestHandler<CloseAdvertisementCommand>
    {
        public async Task Handle(CloseAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement =
                await db.Advertisements
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            
            advertisement.Close(dateTimeService.Now);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("advertisements/{id:guid}/close", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            CloseAdvertisementCommand command = new(id);
            await sender.Send(command, cancellationToken);
            return Results.Ok();
        }).RequireAuthorization();
    }
}
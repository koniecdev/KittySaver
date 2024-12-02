using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class RefreshAdvertisement : IEndpoint
{
    public sealed record RefreshAdvertisementCommand(Guid Id) : ICommand;

    public sealed class RefreshAdvertisementCommandValidator
        : AbstractValidator<RefreshAdvertisementCommand>
    {
        public RefreshAdvertisementCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class RefreshAdvertisementCommandHandler(ApplicationDbContext db, IDateTimeService dateTimeService)
        : IRequestHandler<RefreshAdvertisementCommand>
    {
        public async Task Handle(RefreshAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement =
                await db.Advertisements
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            
            advertisement.Refresh(dateTimeService.Now);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("advertisements/{id:guid}/refresh", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            RefreshAdvertisementCommand command = new(id);
            await sender.Send(command, cancellationToken);
            return Results.Ok();
        }).RequireAuthorization();
    }
}
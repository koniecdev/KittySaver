using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Advertisements;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class ExpireAdvertisement : IEndpoint
{
    public sealed record ExpireAdvertisementCommand(Guid Id) : ICommand;

    public sealed class ExpireAdvertisementCommandValidator
        : AbstractValidator<ExpireAdvertisementCommand>
    {
        public ExpireAdvertisementCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class ExpireAdvertisementCommandHandler(ApplicationDbContext db, IDateTimeService dateTimeService)
        : IRequestHandler<ExpireAdvertisementCommand>
    {
        public async Task Handle(ExpireAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement =
                await db.Advertisements
                    .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            
            advertisement.Expire(dateTimeService.Now);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("advertisements/{id:guid}/expire", async (
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ExpireAdvertisementCommand command = new(id);
            await sender.Send(command, cancellationToken);
            return Results.Ok();
        }).RequireAuthorization();
    }
}
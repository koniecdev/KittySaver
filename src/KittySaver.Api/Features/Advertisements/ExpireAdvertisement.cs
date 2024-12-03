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

    internal sealed class ExpireAdvertisementCommandHandler(
        IAdvertisementRepository advertisementRepository,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
        : IRequestHandler<ExpireAdvertisementCommand>
    {
        public async Task Handle(ExpireAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Advertisement advertisement = await advertisementRepository.GetAdvertisementByIdAsync(request.Id, cancellationToken);
            advertisement.Expire(dateTimeService.Now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
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
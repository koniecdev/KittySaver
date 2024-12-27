using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class CloseAdvertisement : IEndpoint
{
    public sealed record CloseAdvertisementCommand(Guid PersonId, Guid AdvertisementId) : IAdvertisementCommand;

    public sealed class CloseAdvertisementCommandValidator
        : AbstractValidator<CloseAdvertisementCommand>
    {
        public CloseAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.AdvertisementId);
            RuleFor(x => x.AdvertisementId)
                .NotEmpty()
                .NotEqual(x => x.PersonId);
        }
    }

    internal sealed class CloseAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
        : IRequestHandler<CloseAdvertisementCommand>
    {
        public async Task Handle(CloseAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            owner.CloseAdvertisement(request.AdvertisementId, dateTimeService.Now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/advertisements/{advertisementId:guid}/close", async (
            Guid personId,
            Guid advertisementId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            CloseAdvertisementCommand command = new(PersonId: personId, AdvertisementId: advertisementId);
            await sender.Send(command, cancellationToken);
            return Results.Ok();
        }).RequireAuthorization();
    }
}
using FluentValidation;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class RefreshAdvertisement : IEndpoint
{
    public sealed record RefreshAdvertisementCommand(Guid PersonId, Guid AdvertisementId) : ICommand;

    public sealed class RefreshAdvertisementCommandValidator
        : AbstractValidator<RefreshAdvertisementCommand>
    {
        public RefreshAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.AdvertisementId);
            RuleFor(x => x.AdvertisementId)
                .NotEmpty()
                .NotEqual(x => x.PersonId);
        }
    }

    internal sealed class RefreshAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
        : IRequestHandler<RefreshAdvertisementCommand>
    {
        public async Task Handle(RefreshAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            owner.RefreshAdvertisement(request.AdvertisementId, dateTimeService.Now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/advertisements/{advertisementId:guid}/refresh", async (
            Guid personId,
            Guid advertisementId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            RefreshAdvertisementCommand command = new(PersonId: personId, AdvertisementId: advertisementId);
            await sender.Send(command, cancellationToken);
            return Results.Ok();
        }).RequireAuthorization();
    }
}
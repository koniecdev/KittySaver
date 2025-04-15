using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Features.Advertisements;

public sealed class CloseAdvertisement : IEndpoint
{
    public sealed record CloseAdvertisementCommand(PersonId PersonId, AdvertisementId Id)
        : ICommand<AdvertisementHateoasResponse>, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class CloseAdvertisementCommandValidator
        : AbstractValidator<CloseAdvertisementCommand>
    {
        public CloseAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId).NotEmpty();
            RuleFor(x => x.Id).NotEmpty();
        }
    }

    internal sealed class CloseAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
        : IRequestHandler<CloseAdvertisementCommand, AdvertisementHateoasResponse>
    {
        public async Task<AdvertisementHateoasResponse> Handle(CloseAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetByIdAsync(request.PersonId, cancellationToken);
            owner.CloseAdvertisement(request.Id, dateTimeService.Now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            AdvertisementStatus advertisementStatus = owner.Advertisements.First(x => x.Id == request.Id).Status;
            return new AdvertisementHateoasResponse(request.Id, request.PersonId, advertisementStatus);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/advertisements/{id:guid}/close", async (
            Guid personId,
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            CloseAdvertisementCommand command = new(new PersonId(personId), new AdvertisementId(id));
            AdvertisementHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
            return Results.Ok(hateoasResponse);
        }).RequireAuthorization()
        .WithName(EndpointNames.Advertisements.Close.EndpointName)
        .WithTags(EndpointNames.Advertisements.Group);
    }
}
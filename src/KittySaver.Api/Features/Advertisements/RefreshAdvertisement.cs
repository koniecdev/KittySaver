using FluentValidation;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using KittySaver.Shared.Hateoas;
using MediatR;

namespace KittySaver.Api.Features.Advertisements;

public sealed class RefreshAdvertisement : IEndpoint
{
    public sealed record RefreshAdvertisementCommand(Guid PersonId, Guid Id) 
        : ICommand<AdvertisementHateoasResponse>, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class RefreshAdvertisementCommandValidator
        : AbstractValidator<RefreshAdvertisementCommand>
    {
        public RefreshAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.Id);
            RuleFor(x => x.Id)
                .NotEmpty()
                .NotEqual(x => x.PersonId);
        }
    }

    internal sealed class RefreshAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
        : IRequestHandler<RefreshAdvertisementCommand, AdvertisementHateoasResponse>
    {
        public async Task<AdvertisementHateoasResponse> Handle(RefreshAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            owner.RefreshAdvertisement(request.Id, dateTimeService.Now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            AdvertisementStatus advertisementStatus = owner.Advertisements.First(x => x.Id == request.Id).Status;
            return new AdvertisementHateoasResponse(request.Id, request.PersonId, advertisementStatus);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/advertisements/{id:guid}/refresh", async (
            Guid personId,
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            RefreshAdvertisementCommand command = new(PersonId: personId, Id: id);
            AdvertisementHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
            return Results.Ok(hateoasResponse);
        }).RequireAuthorization()
        .WithName(EndpointNames.RefreshAdvertisement.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
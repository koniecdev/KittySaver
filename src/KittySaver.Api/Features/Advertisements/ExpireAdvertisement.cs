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

public sealed class ExpireAdvertisement : IEndpoint
{
    public sealed record ExpireAdvertisementCommand(PersonId PersonId, AdvertisementId Id) 
        : ICommand<AdvertisementHateoasResponse>, IJobOrAdminOnlyRequest, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class ExpireAdvertisementCommandValidator
        : AbstractValidator<ExpireAdvertisementCommand>
    {
        public ExpireAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty();
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }

    internal sealed class ExpireAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IDateTimeService dateTimeService)
        : IRequestHandler<ExpireAdvertisementCommand, AdvertisementHateoasResponse>
    {
        public async Task<AdvertisementHateoasResponse> Handle(ExpireAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            owner.ExpireAdvertisement(request.Id, dateTimeService.Now);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            AdvertisementStatus advertisementStatus = owner.Advertisements.First(x => x.Id == request.Id).Status;
            return new AdvertisementHateoasResponse(request.Id, request.PersonId, advertisementStatus);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPost("persons/{personId:guid}/advertisements/{id:guid}/expire", async (
            Guid personId,
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ExpireAdvertisementCommand command = new(new PersonId(personId), new AdvertisementId(id));
            AdvertisementHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
            return Results.Ok(hateoasResponse);
        }).RequireAuthorization()
        .WithName(EndpointNames.ExpireAdvertisement.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
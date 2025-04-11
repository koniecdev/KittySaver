using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.Requests;
using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Features.Advertisements;

public sealed class ReassignCatsToAdvertisement : IEndpoint
{
    public sealed record ReassignCatsToAdvertisementCommand(
        PersonId PersonId,
        AdvertisementId Id,
        IEnumerable<CatId> CatIds) : ICommand<AdvertisementHateoasResponse>, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class AssignCatToAdvertisementCommandValidator : AbstractValidator<ReassignCatsToAdvertisementCommand>
    {
        public AssignCatToAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                // .WithMessage("'Person Id' cannot be empty.");
                .WithMessage("'Id osoby' nie może być puste.");
            RuleFor(x => x.Id)
                .NotEmpty()
                // .WithMessage("'Id' cannot be empty.");
                .WithMessage("'Id' nie może być puste.");
            RuleFor(x => x.CatIds).NotEmpty()
                // .WithMessage("'Cat Ids' cannot be empty.");
                .WithMessage("'Identyfikatory kotów' nie mogą być puste.");
        }
    }
    
    public sealed class ReassignCatsToAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<ReassignCatsToAdvertisementCommand, AdvertisementHateoasResponse>
    {
        public async Task<AdvertisementHateoasResponse> Handle(ReassignCatsToAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            owner.ReplaceCatsOfAdvertisement(request.Id, request.CatIds);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            AdvertisementStatus advertisementStatus = owner.Advertisements.First(x => x.Id == request.Id).Status;
            return new AdvertisementHateoasResponse(request.Id, request.PersonId, advertisementStatus);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}/advertisements/{id:guid}/cats", async (
            Guid personId,
            Guid id,
            ReassignCatsToAdvertisementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ReassignCatsToAdvertisementCommand command = new(
                new PersonId(personId),
                new AdvertisementId(id),
                request.CatIds);
            AdvertisementHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
            return Results.Ok(hateoasResponse);
        }).RequireAuthorization()
        .WithName(EndpointNames.Advertisements.ReassignCats.EndpointName)
        .WithTags(EndpointNames.Advertisements.Group);
    }
}
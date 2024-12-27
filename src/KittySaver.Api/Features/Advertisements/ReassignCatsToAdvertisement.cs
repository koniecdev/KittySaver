using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Infrastructure.ApiComponents;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;

public sealed class ReassignCatsToAdvertisement : IEndpoint
{
    public sealed record ReassignCatsToAdvertisementRequest(IEnumerable<Guid> CatIds);
    public sealed record ReassignCatsToAdvertisementCommand(
        Guid PersonId,
        Guid AdvertisementId,
        IEnumerable<Guid> CatIds) : IAdvertisementCommand;

    public sealed class AssignCatToAdvertisementCommandValidator : AbstractValidator<ReassignCatsToAdvertisementCommand>
    {
        public AssignCatToAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.AdvertisementId);
            RuleFor(x => x.AdvertisementId)
                .NotEmpty()
                .NotEqual(x => x.PersonId);
            RuleFor(x => x.CatIds).NotEmpty();
        }
    }
    
    public sealed class ReassignCatsToAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork) 
        : IRequestHandler<ReassignCatsToAdvertisementCommand>
    {
        public async Task Handle(ReassignCatsToAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            owner.ReplaceCatsOfAdvertisement(request.AdvertisementId, request.CatIds);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
    
    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}/advertisements/{advertisementId:guid}/cats", async (
            Guid personId,
            Guid advertisementId,
            ReassignCatsToAdvertisementRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            ReassignCatsToAdvertisementCommand command = new(
                PersonId: personId,
                AdvertisementId: advertisementId,
                CatIds: request.CatIds);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
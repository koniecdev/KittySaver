using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using MediatR;

namespace KittySaver.Api.Features.Advertisements;

public sealed class DeleteAdvertisement : IEndpoint
{
    public sealed record DeleteAdvertisementCommand(Guid PersonId, Guid Id) : ICommand, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class DeleteAdvertisementCommandValidator
        : AbstractValidator<DeleteAdvertisementCommand>
    {
        public DeleteAdvertisementCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.Id);
            RuleFor(x => x.Id)
                .NotEmpty()
                .NotEqual(x => x.PersonId);
        }
    }

    internal sealed class DeleteAdvertisementCommandHandler(
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteAdvertisementCommand>
    {
        public async Task Handle(DeleteAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            owner.RemoveAdvertisement(request.Id);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("persons/{personId:guid}/advertisements/{advertisementId:guid}", async (
            Guid personId,
            Guid advertisementId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteAdvertisementCommand command = new(PersonId: personId, Id: advertisementId);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization()
        .WithName(EndpointNames.DeleteAdvertisement.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
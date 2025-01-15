using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
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
        IUnitOfWork unitOfWork,
        IWebHostEnvironment webHostEnvironment)
        : IRequestHandler<DeleteAdvertisementCommand>
    {
        private readonly string _advertisementsBasePath = Path.Combine(
            webHostEnvironment.ContentRootPath,
            "PrivateFiles",
            "advertisements");
        
        public async Task Handle(DeleteAdvertisementCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);
            owner.RemoveAdvertisement(request.Id);
            
            string advertisementPath = Path.Combine(_advertisementsBasePath, request.Id.ToString());
            if (Directory.Exists(advertisementPath))
            {
                Directory.Delete(advertisementPath, recursive: true);
            }
            
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapDelete("persons/{personId:guid}/advertisements/{id:guid}", async (
            Guid personId,
            Guid id,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            DeleteAdvertisementCommand command = new(PersonId: personId, Id: id);
            await sender.Send(command, cancellationToken);
            return Results.NoContent();
        }).RequireAuthorization()
        .WithName(EndpointNames.DeleteAdvertisement.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
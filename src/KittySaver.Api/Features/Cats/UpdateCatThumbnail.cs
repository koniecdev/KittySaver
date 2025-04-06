using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Features.Cats;

public sealed class UpdateCatThumbnail : IEndpoint
{
    public sealed record UpdateCatThumbnailCommand(
        PersonId PersonId,
        CatId Id,
        IFormFile Thumbnail) : ICommand<CatHateoasResponse>, IAuthorizedRequest, ICatRequest;

    public sealed class UpdateAdvertisementThumbnailCommandValidator
        : AbstractValidator<UpdateCatThumbnailCommand>
    {
        public UpdateAdvertisementThumbnailCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty();

            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Thumbnail)
                .NotNull()
                .Must(file => AllowedPictureTypes.AllowedImageTypes
                    .ContainsKey(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage("Only .jpg, .jpeg, .png and .webp files are allowed")
                .Must(file =>
                {
                    string thumbnailType = AllowedPictureTypes
                        .AllowedImageTypes[Path.GetExtension(file.FileName).ToLowerInvariant()];
                    return file.ContentType == thumbnailType;
                }).WithMessage("Only .jpg, .jpeg, .png and .webp files content-types are allowed");
        }
    }

    internal sealed class UpdateCatThumbnailCommandHandler(
        IPersonRepository personRepository,
        ICatThumbnailService catThumbnailService,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateCatThumbnailCommand, CatHateoasResponse>
    {
        public async Task<CatHateoasResponse> Handle(UpdateCatThumbnailCommand request, CancellationToken cancellationToken)
        {
            Person catOwner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);

            catOwner.MarkCatAsThumbnailUploaded(request.Id);
            
            await catThumbnailService.SaveThumbnailAsync(request.Thumbnail, request.Id, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            var catProperties = catOwner.Cats
                .Where(cat => cat.Id == request.Id)
                .Select(c => new
                {
                    advertisementId = c.AdvertisementId,
                    isThumbnailUploaded = c.IsThumbnailUploaded,
                    isAdopted = c.IsAdopted
                }).First();
            
            return new CatHateoasResponse(
                request.Id,
                request.PersonId,
                catProperties.advertisementId,
                catProperties.isThumbnailUploaded,
                catProperties.isAdopted);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}/cats/{id:guid}/thumbnail", async (
                Guid personId,
                Guid id,
                IFormFile? thumbnail,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(thumbnail);
                UpdateCatThumbnailCommand command = new(new PersonId(personId), new CatId(id), thumbnail);
                CatHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
                return Results.Ok(hateoasResponse);
            })
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .RequireAuthorization()
            .WithName(EndpointNames.UpdateCatThumbnail.EndpointName)
            .WithTags(EndpointNames.GroupNames.CatGroup);
    }
}
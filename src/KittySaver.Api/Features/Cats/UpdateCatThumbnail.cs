using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Persons;
using KittySaver.Shared.Hateoas;
using MediatR;

namespace KittySaver.Api.Features.Cats;

public sealed class UpdateCatThumbnail : IEndpoint
{
    public sealed record UpdateCatThumbnailCommand(
        Guid PersonId,
        Guid Id,
        IFormFile Thumbnail) : ICommand<CatHateoasResponse>, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class UpdateAdvertisementThumbnailCommandValidator
        : AbstractValidator<UpdateCatThumbnailCommand>
    {
        public UpdateAdvertisementThumbnailCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.Id);
        
            RuleFor(x => x.Id)
                .NotEmpty()
                .NotEqual(x => x.PersonId);

            RuleFor(x => x.Thumbnail)
                .NotNull()
                .Must(file => IThumbnailStorageService.Constants.AllowedThumbnailTypes
                    .ContainsKey(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage("Only .jpg, .jpeg, .png and .webp files are allowed")
                .Must(file =>
                {
                    string thumbnailType = IThumbnailStorageService.Constants
                        .AllowedThumbnailTypes[Path.GetExtension(file.FileName).ToLowerInvariant()];
                    return file.ContentType == thumbnailType;
                }).WithMessage("Only .jpg, .jpeg, .png and .webp files content-types are allowed");
        }
    }

    internal sealed class UpdateCatThumbnailCommandHandler(
        IPersonRepository personRepository,
        ICatFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateCatThumbnailCommand, CatHateoasResponse>
    {
        public async Task<CatHateoasResponse> Handle(UpdateCatThumbnailCommand request, CancellationToken cancellationToken)
        {
            Person catOwner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);

            catOwner.MarkCatAsThumbnailUploaded(request.Id);
            
            await fileStorage.SaveThumbnailAsync(request.Thumbnail, request.Id, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            var catAdvertisementIdAndIsThumbnailUploaded = catOwner.Cats
                .Where(cat => cat.Id == request.Id)
                .Select(c => new
                {
                    advertisementId = c.AdvertisementId,
                    isThumbnailUploaded = c.IsThumbnailUploaded
                }).First();
            return new CatHateoasResponse(
                request.Id,
                request.PersonId,
                catAdvertisementIdAndIsThumbnailUploaded.advertisementId,
                catAdvertisementIdAndIsThumbnailUploaded.isThumbnailUploaded);
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
                UpdateCatThumbnailCommand command = new(personId, id, thumbnail);
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
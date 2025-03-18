using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using KittySaver.Shared.Hateoas;
using MediatR;

namespace KittySaver.Api.Features.Cats;

public sealed class AddPicturesToCatGallery : IEndpoint
{
    public sealed record AddPicturesToCatGalleryCommand(
        Guid PersonId,
        Guid Id,
        IFormFileCollection GalleryFiles) : ICommand<CatHateoasResponse>, IAuthorizedRequest, ICatRequest;
    
    public sealed class AddPicturesToCatGalleryCommandValidator
        : AbstractValidator<AddPicturesToCatGalleryCommand>
    {
        public AddPicturesToCatGalleryCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.Id);
        
            RuleFor(x => x.Id)
                .NotEmpty()
                .NotEqual(x => x.PersonId);

            RuleFor(x => x.GalleryFiles)
                .NotNull()
                .Must(files => files.Count > 0)
                .WithMessage("At least one gallery file must be provided")
                .Must(files => files.All(file => IGalleryStorageService.Constants.AllowedImageTypes
                    .ContainsKey(Path.GetExtension(file.FileName).ToLowerInvariant())))
                .WithMessage("Only .jpg, .jpeg, .png and .webp files are allowed")
                .Must(files => files.All(file => 
                {
                    string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (!IGalleryStorageService.Constants.AllowedImageTypes.TryGetValue(extension, out string? imageType))
                    {
                        return false;
                    }
                    return file.ContentType == imageType;
                }))
                .WithMessage("Only .jpg, .jpeg, .png and .webp files content-types are allowed");
        }
    }
    
    internal sealed class AddPicturesToCatGalleryCommandHandler(
        IPersonRepository personRepository,
        ICatGalleryService catGalleryService)
        : IRequestHandler<AddPicturesToCatGalleryCommand, CatHateoasResponse>
    {
        public async Task<CatHateoasResponse> Handle(AddPicturesToCatGalleryCommand request, CancellationToken cancellationToken)
        {
            Person catOwner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);

            if (catOwner.Cats.All(x => x.Id != request.Id))
            {
                throw new NotFoundExceptions.CatNotFoundException(request.Id);
            }
            
            await catGalleryService.SaveGalleryImagesAsync(request.GalleryFiles, request.Id, cancellationToken);
            
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
        endpointRouteBuilder.MapPost("persons/{personId:guid}/cats/{id:guid}/gallery", async (
                Guid personId,
                Guid id,
                IFormFileCollection galleryFiles,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(galleryFiles);
                AddPicturesToCatGalleryCommand command = new(personId, id, galleryFiles);
                CatHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
                return Results.Ok(hateoasResponse);
            })
            .DisableAntiforgery()
            .Accepts<IFormFileCollection>("multipart/form-data")
            .RequireAuthorization()
            .WithName(EndpointNames.AddPicturesToCatGallery.EndpointName)
            .WithTags(EndpointNames.GroupNames.CatGroup);
    }
}
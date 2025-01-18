using FluentValidation;
using KittySaver.Api.Features.Advertisements.SharedContracts;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Hateoas;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.ValueObjects;
using MediatR;
using Riok.Mapperly.Abstractions;

namespace KittySaver.Api.Features.Advertisements;

public sealed class UpdateAdvertisementThumbnail : IEndpoint
{
    public sealed record UpdateAdvertisementThumbnailCommand(
        Guid PersonId,
        Guid Id,
        IFormFile Thumbnail) : ICommand<AdvertisementHateoasResponse>, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class UpdateAdvertisementThumbnailCommandValidator
        : AbstractValidator<UpdateAdvertisementThumbnailCommand>
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
                .Must(file => file.Length <= IThumbnailStorageService.Constants.MaxFileSizeBytes)
                .WithMessage("File size must not exceed 5MB")
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

    internal sealed class UpdateAdvertisementThumbnailCommandHandler(
        IPersonRepository personRepository,
        IAdvertisementFileStorageService fileStorage,
        IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateAdvertisementThumbnailCommand, AdvertisementHateoasResponse>
    {
        public async Task<AdvertisementHateoasResponse> Handle(
            UpdateAdvertisementThumbnailCommand request, 
            CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);

            owner.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(request.Id);
            
            await fileStorage.SaveThumbnailAsync(request.Thumbnail, request.Id, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            Advertisement.AdvertisementStatus advertisementStatus = owner.Advertisements
                .First(x => x.Id == request.Id)
                .Status;
                
            return new AdvertisementHateoasResponse(
                request.Id, 
                request.PersonId, 
                (AdvertisementResponse.AdvertisementStatus)advertisementStatus);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}/advertisements/{id:guid}/thumbnail", async (
                Guid personId,
                Guid id,
                IFormFile? thumbnail,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(thumbnail);
                UpdateAdvertisementThumbnailCommand command = new(personId, id, thumbnail);
                AdvertisementHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
                return Results.Ok(hateoasResponse);
            })
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .RequireAuthorization()
            .WithName(EndpointNames.UpdateAdvertisementThumbnail.EndpointName)
            .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}
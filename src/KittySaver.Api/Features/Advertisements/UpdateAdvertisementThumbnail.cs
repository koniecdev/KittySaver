﻿using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services.FileServices;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Features.Advertisements;

public sealed class UpdateAdvertisementThumbnail : IEndpoint
{
    public sealed record UpdateAdvertisementThumbnailCommand(
        PersonId PersonId,
        AdvertisementId Id,
        IFormFile Thumbnail) : ICommand<AdvertisementHateoasResponse>, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class UpdateAdvertisementThumbnailCommandValidator : AbstractValidator<UpdateAdvertisementThumbnailCommand>
    {
        public UpdateAdvertisementThumbnailCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                // .WithMessage("'Person Id' cannot be empty.");
                .WithMessage("'Id osoby' nie może być puste.");

            RuleFor(x => x.Id)
                .NotEmpty()
                // .WithMessage("'Id' cannot be empty.");
                .WithMessage("'Id' nie może być puste.");

            RuleFor(x => x.Thumbnail)
                .NotNull()
                // .WithMessage("'Thumbnail' cannot be null.")
                .WithMessage("'Miniatura' nie może być pusta.")
                .Must(file => AllowedPictureTypes.AllowedImageTypes
                    .ContainsKey(Path.GetExtension(file.FileName).ToLowerInvariant()))
                // .WithMessage("Only .jpg, .jpeg, .png and .webp files are allowed")
                .WithMessage("Dozwolone są tylko pliki .jpg, .jpeg, .png i .webp")
                .Must(file =>
                {
                    string thumbnailType = AllowedPictureTypes
                        .AllowedImageTypes[Path.GetExtension(file.FileName).ToLowerInvariant()];
                    return file.ContentType == thumbnailType;
                })
                // .WithMessage("Only .jpg, .jpeg, .png and .webp files content-types are allowed");
                .WithMessage("Dozwolone są tylko typy treści plików .jpg, .jpeg, .png i .webp");
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
            Person owner = await personRepository.GetByIdAsync(request.PersonId, cancellationToken);

            owner.ActivateAdvertisementIfThumbnailIsUploadedForTheFirstTime(request.Id);
            
            await fileStorage.SaveThumbnailAsync(request.Thumbnail, request.Id, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            AdvertisementStatus advertisementStatus = owner.Advertisements
                .First(x => x.Id == request.Id)
                .Status;
                
            return new AdvertisementHateoasResponse(
                request.Id, 
                request.PersonId, 
                advertisementStatus);
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
                UpdateAdvertisementThumbnailCommand command = new(new PersonId(personId), new AdvertisementId(id), thumbnail);
                AdvertisementHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
                return Results.Ok(hateoasResponse);
            })
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .RequireAuthorization()
            .WithName(EndpointNames.Advertisements.UpdateThumbnail.EndpointName)
            .WithTags(EndpointNames.Advertisements.Group);
    }
}
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
    // public sealed record UpdateAdvertisementThumbnailRequest(
    //     IFormFile Thumbnail);

    public sealed record UpdateAdvertisementThumbnailCommand(
        Guid PersonId,
        Guid Id,
        IFormFile Thumbnail) : ICommand<AdvertisementHateoasResponse>, IAuthorizedRequest, IAdvertisementRequest;

    public sealed class UpdateAdvertisementThumbnailCommandValidator
        : AbstractValidator<UpdateAdvertisementThumbnailCommand>
    {
        private readonly string[] _allowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
        private const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

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
                .Must(file => file.Length <= MaxFileSizeBytes)
                .WithMessage("File size must not exceed 5MB")
                .Must(file => _allowedExtensions.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage("Only .jpg, .jpeg and .png files are allowed");
        }
    }

    internal sealed class UpdateAdvertisementThumbnailCommandHandler(
        IPersonRepository personRepository,
        IFileStorageService fileStorage)
        : IRequestHandler<UpdateAdvertisementThumbnailCommand, AdvertisementHateoasResponse>
    {
        public async Task<AdvertisementHateoasResponse> Handle(UpdateAdvertisementThumbnailCommand request, CancellationToken cancellationToken)
        {
            Person owner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);

            if (owner.Advertisements.All(x => x.Id != request.Id))
            {
                throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            }
            
            await using Stream stream = request.Thumbnail.OpenReadStream();
            string fileName = $"{request.Id}_thumbnail{Path.GetExtension(request.Thumbnail.FileName)}";
            await fileStorage.SaveFileAsync(stream, fileName, cancellationToken);
            
            Advertisement.AdvertisementStatus advertisementStatus = owner.Advertisements.First(x => x.Id == request.Id).Status;
            return new AdvertisementHateoasResponse(request.Id, request.PersonId, (AdvertisementResponse.AdvertisementStatus)advertisementStatus);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapPut("persons/{personId:guid}/advertisements/{id:guid}/thumbnail", async (
            Guid personId,
            Guid id,
            IFormFile thumbnail,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            UpdateAdvertisementThumbnailCommand command = new(personId, id, thumbnail);
            AdvertisementHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
            return Results.Ok(hateoasResponse);
        })
        .DisableAntiforgery()
        .Accepts<IFormFile>("multipart/form-data").RequireAuthorization()
        .WithName(EndpointNames.UpdateAdvertisementThumbnail.EndpointName)
        .WithTags(EndpointNames.GroupNames.AdvertisementGroup);
    }
}

// [Mapper]
// public static partial class UpdateAdvertisementThumbnailMapper
// {
//     public static partial UpdateAdvertisementThumbnail.UpdateAdvertisementThumbnailCommand MapToUpdateAdvertisementThumbnailCommand(
//         this UpdateAdvertisementThumbnail.UpdateAdvertisementThumbnailRequest request,
//         Guid personId,
//         Guid id);
// }
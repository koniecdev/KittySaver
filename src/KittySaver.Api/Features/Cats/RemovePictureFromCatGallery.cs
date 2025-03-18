using FluentValidation;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using KittySaver.Shared.Hateoas;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Features.Cats;

public sealed class RemovePictureFromCatGallery : IEndpoint
{
    public sealed record RemovePictureFromCatGalleryCommand(
        Guid PersonId,
        Guid Id,
        string FileNameWithExtension) : ICommand<CatHateoasResponse>, IAuthorizedRequest, ICatRequest;
    
    public sealed class RemovePictureFromCatGalleryCommandValidator
        : AbstractValidator<RemovePictureFromCatGalleryCommand>
    {
        public RemovePictureFromCatGalleryCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                .NotEqual(x => x.Id);
        
            RuleFor(x => x.Id)
                .NotEmpty()
                .NotEqual(x => x.PersonId);

            RuleFor(x => x.FileNameWithExtension).NotNull();
        }
    }
    
    internal sealed class RemovePictureFromCatGalleryCommandHandler(
        IPersonRepository personRepository,
        ICatGalleryService catGalleryService)
        : IRequestHandler<RemovePictureFromCatGalleryCommand, CatHateoasResponse>
    {
        public async Task<CatHateoasResponse> Handle(RemovePictureFromCatGalleryCommand request, CancellationToken cancellationToken)
        {
            Person catOwner = await personRepository.GetPersonByIdAsync(request.PersonId, cancellationToken);

            if (catOwner.Cats.All(x => x.Id != request.Id))
            {
                throw new NotFoundExceptions.CatNotFoundException(request.Id);
            }

            catGalleryService.DeleteGalleryImage(request.Id, request.FileNameWithExtension);
            
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
        endpointRouteBuilder.MapDelete("persons/{personId:guid}/cats/{id:guid}/gallery/{filename}", async (
                Guid personId,
                Guid id,
                string filename,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                ArgumentNullException.ThrowIfNull(filename);
                RemovePictureFromCatGalleryCommand command = new(personId, id, filename);
                CatHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
                return Results.Ok(hateoasResponse);
            })
            .RequireAuthorization()
            .WithName(EndpointNames.RemovePictureFromCatGallery.EndpointName)
            .WithTags(EndpointNames.GroupNames.CatGroup);
    }
}
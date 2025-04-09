using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KittySaver.Api.Features.Cats;

public sealed class RemovePictureFromCatGallery : IEndpoint
{
    public sealed record RemovePictureFromCatGalleryCommand(
        PersonId PersonId,
        CatId Id,
        string FileNameWithExtension) : ICommand<CatHateoasResponse>, IAuthorizedRequest, ICatRequest;
    
    public sealed class RemovePictureFromCatGalleryCommandValidator
        : AbstractValidator<RemovePictureFromCatGalleryCommand>
    {
        public RemovePictureFromCatGalleryCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty();

            RuleFor(x => x.Id)
                .NotEmpty();

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
                RemovePictureFromCatGalleryCommand command = new(new PersonId(personId), new CatId(id), filename);
                CatHateoasResponse hateoasResponse = await sender.Send(command, cancellationToken);
                return Results.Ok(hateoasResponse);
            })
            .RequireAuthorization()
            .WithName(EndpointNames.RemovePictureFromCatGallery.EndpointName)
            .WithTags(EndpointNames.GroupNames.CatGroup);
    }
}
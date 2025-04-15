using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons.DomainRepositories;
using KittySaver.Domain.Persons.Entities;
using KittySaver.Shared.Hateoas;
using KittySaver.Shared.TypedIds;
using MediatR;

namespace KittySaver.Api.Features.Cats;

public sealed class RemovePictureFromCatGallery : IEndpoint
{
    public sealed record RemovePictureFromCatGalleryCommand(
        PersonId PersonId,
        CatId Id,
        string FileNameWithExtension) : ICommand<CatHateoasResponse>, IAuthorizedRequest, ICatRequest;
    
    public sealed class RemovePictureFromCatGalleryCommandValidator : AbstractValidator<RemovePictureFromCatGalleryCommand>
    {
        public RemovePictureFromCatGalleryCommandValidator()
        {
            RuleFor(x => x.PersonId)
                .NotEmpty()
                // .WithMessage("'Person Id' cannot be empty.");
                .WithMessage("'Id osoby' nie może być puste.");

            RuleFor(x => x.Id)
                .NotEmpty()
                // .WithMessage("'Id' cannot be empty.");
                .WithMessage("'Id' nie może być puste.");

            RuleFor(x => x.FileNameWithExtension).NotNull()
                // .WithMessage("'File Name With Extension' cannot be null.");
                .WithMessage("'Nazwa pliku z rozszerzeniem' nie może być pusta.");
        }
    }
    
    internal sealed class RemovePictureFromCatGalleryCommandHandler(
        IPersonRepository personRepository,
        ICatGalleryService catGalleryService)
        : IRequestHandler<RemovePictureFromCatGalleryCommand, CatHateoasResponse>
    {
        public async Task<CatHateoasResponse> Handle(RemovePictureFromCatGalleryCommand request, CancellationToken cancellationToken)
        {
            Person catOwner = await personRepository.GetByIdAsync(request.PersonId, cancellationToken);

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
            .WithName(EndpointNames.Cats.RemovePicture.EndpointName)
            .WithTags(EndpointNames.Cats.Group);
    }
}
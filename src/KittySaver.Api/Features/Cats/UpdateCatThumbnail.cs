using FluentValidation;
using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services.FileServices;
using KittySaver.Api.Persistence.WriteRelated;
using KittySaver.Api.Shared.Abstractions;
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

    public sealed class UpdateCatThumbnailCommandValidator : AbstractValidator<UpdateCatThumbnailCommand>
    {
        public UpdateCatThumbnailCommandValidator()
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
            .WithName(EndpointNames.Cats.UpdateThumbnail.EndpointName)
            .WithTags(EndpointNames.Cats.Group);
    }
}
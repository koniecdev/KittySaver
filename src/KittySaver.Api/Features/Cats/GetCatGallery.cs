using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services.FileServices;
using KittySaver.Api.Persistence.ReadRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.Responses;
using KittySaver.Shared.TypedIds;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;

public sealed class GetCatGallery : IEndpoint
{
    public sealed record GetCatGalleryQuery(PersonId PersonId, CatId Id) : IQuery<ICollection<PictureResponse>>;

    internal sealed class GetCatGalleryQueryHandler(
        ApplicationReadDbContext db,
        ICatGalleryService catGalleryService,
        LinkGenerator linkGenerator)
        : IRequestHandler<GetCatGalleryQuery, ICollection<PictureResponse>>
    {
        public async Task<ICollection<PictureResponse>> Handle(
            GetCatGalleryQuery request, 
            CancellationToken cancellationToken)
        {
            bool catExists = await db.Cats
                .AnyAsync(x => x.PersonId == request.PersonId && x.Id == request.Id, cancellationToken);
            
            if (!catExists)
            {
                throw new NotFoundExceptions.CatNotFoundException(request.Id);
            }

            IDictionary<string, string> imagePaths = catGalleryService.GetGalleryImagePaths(request.Id);
            
            List<PictureResponse> images = imagePaths.Keys
                .Select(filename => new PictureResponse(filename))
                .ToList();
            
            return images;
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/persons/{personId:guid}/cats/{id:guid}/gallery", async (
                Guid personId,
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                GetCatGalleryQuery query = new(new PersonId(personId), new CatId(id));
                ICollection<PictureResponse> response = await sender.Send(query, cancellationToken);
                return Results.Ok(response);
            })
            .AllowAnonymous()
            .WithName(EndpointNames.Cats.GetGallery.EndpointName)
            .WithTags(EndpointNames.Cats.Group)
            .Produces<ICollection<PictureResponse>>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
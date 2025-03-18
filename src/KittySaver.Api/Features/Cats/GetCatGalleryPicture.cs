using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;
public sealed class GetCatGalleryPicture : IEndpoint
{
    public sealed record GetCatGalleryPictureQuery(Guid PersonId, Guid Id, string Filename) : IQuery<(FileStream Stream, string ContentType)>;

    internal sealed class GetCatGalleryPictureQueryHandler(
        ApplicationReadDbContext db,
        ICatGalleryService catGalleryService)
        : IRequestHandler<GetCatGalleryPictureQuery, (FileStream Stream, string ContentType)>
    {
        public async Task<(FileStream Stream, string ContentType)> Handle(
            GetCatGalleryPictureQuery request, 
            CancellationToken cancellationToken)
        {
            if (!await db.Cats
                    .Where(x => x.PersonId == request.PersonId && x.Id == request.Id)
                    .AnyAsync(cancellationToken))
            {
                throw new NotFoundExceptions.CatNotFoundException(request.Id);
            }

            FileStream fileStream = catGalleryService.GetGalleryImage(request.Id, request.Filename);
            string contentType = catGalleryService.GetContentType(fileStream.Name);
        
            return (fileStream, contentType);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/persons/{personId:guid}/cats/{id:guid}/gallery/{filename}", async (
                Guid personId,
                Guid id,
                string filename,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                GetCatGalleryPictureQuery query = new(personId, id, filename);
                (FileStream fileStream, string contentType) = await sender.Send(query, cancellationToken);
                
                return Results.Stream(
                    fileStream,
                    contentType: contentType,
                    enableRangeProcessing: true);
            })
            .AllowAnonymous()
            .WithName(EndpointNames.GetCatGalleryPicture.EndpointName)
            .WithTags(EndpointNames.GroupNames.CatGroup)
            .Produces<FileStream>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
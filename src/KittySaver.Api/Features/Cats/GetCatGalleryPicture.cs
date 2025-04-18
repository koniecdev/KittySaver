﻿using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services.FileServices;
using KittySaver.Api.Persistence.ReadRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.TypedIds;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;
public sealed class GetCatGalleryPicture : IEndpoint
{
    public sealed record GetCatGalleryPictureQuery(PersonId PersonId, CatId Id, string Filename) : IQuery<(FileStream Stream, string ContentType)>;

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
                GetCatGalleryPictureQuery query = new(new PersonId(personId), new CatId(id), filename);
                (FileStream fileStream, string contentType) = await sender.Send(query, cancellationToken);
                
                return Results.Stream(
                    fileStream,
                    contentType: contentType,
                    enableRangeProcessing: true);
            })
            .AllowAnonymous()
            .WithName(EndpointNames.Cats.GetGalleryPicture.EndpointName)
            .WithTags(EndpointNames.Cats.Group)
            .Produces<FileStream>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
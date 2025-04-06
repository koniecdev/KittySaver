using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.TypedIds;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Cats;
public sealed class GetCatThumbnail : IEndpoint
{
    public sealed record GetCatThumbnailQuery(PersonId PersonId, CatId Id) : IQuery<(FileStream Stream, string ContentType)>;

    internal sealed class GetCatThumbnailQueryHandler(
        ApplicationReadDbContext db,
        ICatThumbnailService catThumbnailService)
        : IRequestHandler<GetCatThumbnailQuery, (FileStream Stream, string ContentType)>
    {
        public async Task<(FileStream Stream, string ContentType)> Handle(
            GetCatThumbnailQuery request, 
            CancellationToken cancellationToken)
        {
            bool? isThumbnailUploaded = await db.Cats
                .Where(x => x.PersonId == request.PersonId && x.Id == request.Id)
                .Select(x=>(bool?)x.IsThumbnailUploaded)
                .FirstOrDefaultAsync(cancellationToken);
            
            switch (isThumbnailUploaded)
            {
                case null:
                    throw new NotFoundExceptions.CatNotFoundException(request.Id);
                case false:
                    throw new InvalidOperationException("Thumbnail is not uploaded");
            }

            FileStream fileStream = catThumbnailService.GetThumbnail(request.Id);
            string contentType = catThumbnailService.GetContentType(fileStream.Name);
        
            return (fileStream, contentType);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/persons/{personId:guid}/cats/{id:guid}/thumbnail", async (
                Guid personId,
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                GetCatThumbnailQuery query = new(new PersonId(personId), new CatId(id));
                (FileStream fileStream, string contentType) = await sender.Send(query, cancellationToken);
                
                return Results.Stream(
                    fileStream,
                    contentType: contentType,
                    enableRangeProcessing: true);
            })
            .AllowAnonymous()
            .WithName(EndpointNames.GetCatThumbnail.EndpointName)
            .WithTags(EndpointNames.GroupNames.CatGroup)
            .Produces<FileStream>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
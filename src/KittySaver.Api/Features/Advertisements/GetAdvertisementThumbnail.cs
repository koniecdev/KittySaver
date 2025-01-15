using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;
public sealed class GetAdvertisementThumbnail : IEndpoint
{
    public sealed record GetAdvertisementThumbnailQuery(Guid Id) : IQuery<(FileStream Stream, string ContentType)>;

    internal sealed class GetAdvertisementThumbnailQueryHandler(
        ApplicationReadDbContext db,
        IAdvertisementFileStorageService fileStorage)
        : IRequestHandler<GetAdvertisementThumbnailQuery, (FileStream Stream, string ContentType)>
    {
        public async Task<(FileStream Stream, string ContentType)> Handle(
            GetAdvertisementThumbnailQuery request, 
            CancellationToken cancellationToken)
        {
            bool exists = await db.Advertisements
                .AnyAsync(x => x.Id == request.Id, cancellationToken);
            
            if (!exists)
            {
                throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            }
        
            FileStream fileStream = fileStorage.GetThumbnail(request.Id);
            string contentType = fileStorage.GetContentType(fileStream.Name);
        
            return (fileStream, contentType);
        }
    }

    public void MapEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("advertisements/{id:guid}/thumbnail", async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                GetAdvertisementThumbnailQuery query = new(id);
                (FileStream fileStream, string contentType) = await sender.Send(query, cancellationToken);
                
                return Results.Stream(
                    fileStream,
                    contentType: contentType,
                    enableRangeProcessing: true);
            })
            .AllowAnonymous()
            .WithName(EndpointNames.GetAdvertisementThumbnail.EndpointName)
            .WithTags(EndpointNames.GroupNames.AdvertisementGroup)
            .Produces<FileStream>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
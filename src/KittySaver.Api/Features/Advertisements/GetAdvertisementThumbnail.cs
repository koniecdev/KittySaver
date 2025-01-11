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
        IFileStorageService fileStorage)
        : IRequestHandler<GetAdvertisementThumbnailQuery, (FileStream Stream, string ContentType)>
    {
        private static readonly Dictionary<string, string> ContentTypes = new()
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };

        public async Task<(FileStream Stream, string ContentType)> Handle(GetAdvertisementThumbnailQuery request, CancellationToken cancellationToken)
        {
            if (await db.Advertisements.AllAsync(x => x.Id != request.Id, cancellationToken))
            {
                throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
            }
            
            FileStream fileStream = await fileStorage.GetFileAsync($"{request.Id}_thumbnail", cancellationToken);
            
            string extension = Path.GetExtension(fileStream.Name).ToLowerInvariant();
            string contentType = ContentTypes.GetValueOrDefault(extension, "application/octet-stream");
            
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
                GetAdvertisementThumbnailQuery query = new GetAdvertisementThumbnailQuery(id);
                (FileStream fileStream, string contentType) = await sender.Send(query, cancellationToken);
                    
                return Results.File(
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
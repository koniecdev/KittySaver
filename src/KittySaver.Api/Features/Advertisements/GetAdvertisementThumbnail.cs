using KittySaver.Api.Infrastructure.Endpoints;
using KittySaver.Api.Infrastructure.Services.FileServices;
using KittySaver.Api.Persistence.ReadRelated;
using KittySaver.Api.Shared.Abstractions;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Shared.Common.Enums;
using KittySaver.Shared.TypedIds;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KittySaver.Api.Features.Advertisements;
public sealed class GetAdvertisementThumbnail : IEndpoint
{
    public sealed record GetAdvertisementThumbnailQuery(AdvertisementId Id) : IQuery<(FileStream Stream, string ContentType)>;

    internal sealed class GetAdvertisementThumbnailQueryHandler(
        ApplicationReadDbContext db,
        IAdvertisementFileStorageService fileStorage)
        : IRequestHandler<GetAdvertisementThumbnailQuery, (FileStream Stream, string ContentType)>
    {
        public async Task<(FileStream Stream, string ContentType)> Handle(
            GetAdvertisementThumbnailQuery request, 
            CancellationToken cancellationToken)
        {
            AdvertisementStatus? status = await db.Advertisements
                .Where(x => x.Id == request.Id)
                .Select(x=>(AdvertisementStatus?)x.Status)
                .FirstOrDefaultAsync(cancellationToken);
            
            switch (status)
            {
                case null:
                    throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
                case AdvertisementStatus.ThumbnailNotUploaded:
                    throw new InvalidOperationException("Thumbnail is not uploaded");
                case AdvertisementStatus.Active:
                case AdvertisementStatus.Closed:
                case AdvertisementStatus.Expired:
                    break;
                default:
                    throw new IndexOutOfRangeException();
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
                GetAdvertisementThumbnailQuery query = new(new AdvertisementId(id));
                (FileStream fileStream, string contentType) = await sender.Send(query, cancellationToken);
                
                return Results.Stream(
                    fileStream,
                    contentType: contentType,
                    enableRangeProcessing: true);
            })
            .AllowAnonymous()
            .WithName(EndpointNames.Advertisements.GetThumbnail.EndpointName)
            .WithTags(EndpointNames.Advertisements.Group)
            .Produces<FileStream>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
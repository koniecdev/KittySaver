using KittySaver.Api.Shared.Abstractions;
using KittySaver.Api.Shared.Endpoints;
using KittySaver.Api.Shared.Infrastructure.Services;
using KittySaver.Api.Shared.Infrastructure.Services.FileServices;
using KittySaver.Api.Shared.Persistence;
using KittySaver.Domain.Common.Exceptions;
using KittySaver.Domain.Persons;
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
            Advertisement.AdvertisementStatus? status = await db.Advertisements
                .Where(x => x.Id == request.Id)
                .Select(x=>(Advertisement.AdvertisementStatus?)x.Status)
                .FirstOrDefaultAsync(cancellationToken);
            
            switch (status)
            {
                case null:
                    throw new NotFoundExceptions.AdvertisementNotFoundException(request.Id);
                case Advertisement.AdvertisementStatus.ThumbnailNotUploaded:
                    throw new InvalidOperationException("Thumbnail is not uploaded");
                case Advertisement.AdvertisementStatus.Active:
                case Advertisement.AdvertisementStatus.Closed:
                case Advertisement.AdvertisementStatus.Expired:
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
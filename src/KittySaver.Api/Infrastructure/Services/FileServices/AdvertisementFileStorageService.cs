using KittySaver.Shared.TypedIds;

namespace KittySaver.Api.Infrastructure.Services.FileServices;

public interface IAdvertisementFileStorageService
{
    Task SaveThumbnailAsync(IFormFile sourceFile, AdvertisementId advertisementId, CancellationToken cancellationToken);
    FileStream GetThumbnail(AdvertisementId advertisementId);
    string GetContentType(string fileName);
    void DeleteThumbnail(AdvertisementId advertisementId);
}

public class AdvertisementFileStorageService(IThumbnailStorageService<AdvertisementId> thumbnailStorage)
    : IAdvertisementFileStorageService
{
    private const string EntityType = "advertisements";

    public Task SaveThumbnailAsync(IFormFile sourceFile, AdvertisementId advertisementId, CancellationToken cancellationToken) =>
        thumbnailStorage.SaveThumbnailAsync(sourceFile, EntityType, advertisementId, cancellationToken);

    public FileStream GetThumbnail(AdvertisementId advertisementId) =>
        thumbnailStorage.GetThumbnail(EntityType, advertisementId);
        
    public string GetContentType(string fileName) =>
        thumbnailStorage.GetContentType(fileName);

    public void DeleteThumbnail(AdvertisementId advertisementId)
    {
        thumbnailStorage.DeleteThumbnail(EntityType, advertisementId);
    }
}
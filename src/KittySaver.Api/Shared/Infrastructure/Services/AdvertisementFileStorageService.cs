namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface IAdvertisementFileStorageService
{
    Task SaveThumbnailAsync(IFormFile sourceFile, Guid advertisementId, CancellationToken cancellationToken);
    FileStream GetThumbnail(Guid advertisementId);
    string GetContentType(string fileName);
}

public class AdvertisementFileStorageService(IThumbnailStorageService thumbnailStorage)
    : IAdvertisementFileStorageService
{
    private const string EntityType = "advertisements";

    public Task SaveThumbnailAsync(IFormFile sourceFile, Guid advertisementId, CancellationToken cancellationToken) =>
        thumbnailStorage.SaveThumbnailAsync(sourceFile, EntityType, advertisementId, cancellationToken);

    public FileStream GetThumbnail(Guid advertisementId) =>
        thumbnailStorage.GetThumbnail(EntityType, advertisementId);
        
    public string GetContentType(string fileName) =>
        thumbnailStorage.GetContentType(fileName);
}
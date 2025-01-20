namespace KittySaver.Api.Shared.Infrastructure.Services.FileServices;

public interface ICatFileStorageService
{
    Task SaveThumbnailAsync(IFormFile sourceFile, Guid advertisementId, CancellationToken cancellationToken);
    FileStream GetThumbnail(Guid advertisementId);
    string GetContentType(string fileName);
    void DeleteThumbnail(Guid advertisementId);
}

public class CatFileStorageService(IThumbnailStorageService thumbnailStorage)
    : ICatFileStorageService
{
    private const string EntityType = "cats";

    public Task SaveThumbnailAsync(IFormFile sourceFile, Guid advertisementId, CancellationToken cancellationToken) =>
        thumbnailStorage.SaveThumbnailAsync(sourceFile, EntityType, advertisementId, cancellationToken);

    public FileStream GetThumbnail(Guid advertisementId) =>
        thumbnailStorage.GetThumbnail(EntityType, advertisementId);
        
    public string GetContentType(string fileName) =>
        thumbnailStorage.GetContentType(fileName);

    public void DeleteThumbnail(Guid advertisementId)
    {
        thumbnailStorage.DeleteThumbnail(EntityType, advertisementId);
    }
}
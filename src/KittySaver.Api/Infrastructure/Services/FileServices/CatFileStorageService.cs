namespace KittySaver.Api.Shared.Infrastructure.Services.FileServices;

public interface ICatThumbnailService
{
    Task SaveThumbnailAsync(IFormFile sourceFile, Guid advertisementId, CancellationToken cancellationToken);
    FileStream GetThumbnail(Guid advertisementId);
    string GetContentType(string fileName);
    void DeleteThumbnail(Guid advertisementId);
}

public interface ICatGalleryService
{
    Task<IReadOnlyList<string>> SaveGalleryImagesAsync(IEnumerable<IFormFile> sourceFiles, Guid catId, CancellationToken cancellationToken);
    IReadOnlyList<FileStream> GetGalleryImages(Guid catId);
    FileStream GetGalleryImage(Guid catId, string filename);
    IDictionary<string, string> GetGalleryImagePaths(Guid catId);
    void DeleteGalleryImage(Guid catId, string filename);
    void DeleteAllGalleryImages(Guid catId);
    public string GetContentType(string fileName);
}

public class CatFileStorageService(IThumbnailStorageService thumbnailStorage, IGalleryStorageService galleryStorage)
    : ICatThumbnailService, ICatGalleryService
{
    private const string EntityType = "cats";

    public Task SaveThumbnailAsync(IFormFile sourceFile, Guid advertisementId, CancellationToken cancellationToken) =>
        thumbnailStorage.SaveThumbnailAsync(sourceFile, EntityType, advertisementId, cancellationToken);

    public FileStream GetThumbnail(Guid advertisementId) =>
        thumbnailStorage.GetThumbnail(EntityType, advertisementId);
        
    public string GetContentType(string fileName) =>
        thumbnailStorage.GetContentType(fileName);

    public void DeleteThumbnail(Guid advertisementId) => thumbnailStorage.DeleteThumbnail(EntityType, advertisementId);

    public Task<IReadOnlyList<string>> SaveGalleryImagesAsync(
        IEnumerable<IFormFile> sourceFiles, 
        Guid catId, 
        CancellationToken cancellationToken) =>
        galleryStorage.SaveGalleryImagesAsync(sourceFiles, EntityType, catId, cancellationToken);

    public IReadOnlyList<FileStream> GetGalleryImages(Guid catId) =>
        galleryStorage.GetGalleryImages(EntityType, catId);

    public FileStream GetGalleryImage(Guid catId, string filename) =>
        galleryStorage.GetGalleryImage(EntityType, catId, filename);

    public IDictionary<string, string> GetGalleryImagePaths(Guid catId) =>
        galleryStorage.GetGalleryImagePaths(EntityType, catId);
    
    public void DeleteGalleryImage(Guid catId, string filename) =>
        galleryStorage.DeleteGalleryImage(EntityType, catId, filename);

    public void DeleteAllGalleryImages(Guid catId) =>
        galleryStorage.DeleteAllGalleryImages(EntityType, catId);
}
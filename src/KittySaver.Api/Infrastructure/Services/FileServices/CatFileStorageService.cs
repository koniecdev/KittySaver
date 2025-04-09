using KittySaver.Shared.TypedIds;

namespace KittySaver.Api.Infrastructure.Services.FileServices;

public interface ICatThumbnailService
{
    Task SaveThumbnailAsync(IFormFile sourceFile, CatId catId, CancellationToken cancellationToken);
    FileStream GetThumbnail(CatId catId);
    string GetContentType(string fileName);
    void DeleteThumbnail(CatId catId);
}

public interface ICatGalleryService
{
    Task<IReadOnlyList<string>> SaveGalleryImagesAsync(IEnumerable<IFormFile> sourceFiles, CatId catId, CancellationToken cancellationToken);
    IReadOnlyList<FileStream> GetGalleryImages(CatId catId);
    FileStream GetGalleryImage(CatId catId, string filename);
    IDictionary<string, string> GetGalleryImagePaths(CatId catId);
    void DeleteGalleryImage(CatId catId, string filename);
    void DeleteAllGalleryImages(CatId catId);
    public string GetContentType(string fileName);
}

public class CatFileStorageService(IThumbnailStorageService<CatId> thumbnailStorage, IGalleryStorageService<CatId> galleryStorage)
    : ICatThumbnailService, ICatGalleryService
{
    private const string EntityType = "cats";

    public Task SaveThumbnailAsync(IFormFile sourceFile, CatId catId, CancellationToken cancellationToken) =>
        thumbnailStorage.SaveThumbnailAsync(sourceFile, EntityType, catId, cancellationToken);

    public FileStream GetThumbnail(CatId catId) =>
        thumbnailStorage.GetThumbnail(EntityType, catId);
        
    public string GetContentType(string fileName) =>
        thumbnailStorage.GetContentType(fileName);

    public void DeleteThumbnail(CatId catId) => thumbnailStorage.DeleteThumbnail(EntityType, catId);

    public Task<IReadOnlyList<string>> SaveGalleryImagesAsync(
        IEnumerable<IFormFile> sourceFiles, 
        CatId catId, 
        CancellationToken cancellationToken) =>
        galleryStorage.SaveGalleryImagesAsync(sourceFiles, EntityType, catId, cancellationToken);

    public IReadOnlyList<FileStream> GetGalleryImages(CatId catId) =>
        galleryStorage.GetGalleryImages(EntityType, catId);

    public FileStream GetGalleryImage(CatId catId, string filename) =>
        galleryStorage.GetGalleryImage(EntityType, catId, filename);

    public IDictionary<string, string> GetGalleryImagePaths(CatId catId) =>
        galleryStorage.GetGalleryImagePaths(EntityType, catId);
    
    public void DeleteGalleryImage(CatId catId, string filename) =>
        galleryStorage.DeleteGalleryImage(EntityType, catId, filename);

    public void DeleteAllGalleryImages(CatId catId) =>
        galleryStorage.DeleteAllGalleryImages(EntityType, catId);
}
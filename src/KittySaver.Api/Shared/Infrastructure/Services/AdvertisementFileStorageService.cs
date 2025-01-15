namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface IAdvertisementFileStorageService
{
    Task SaveThumbnailAsync(Stream sourceStream, Guid advertisementId, string fileExtension, CancellationToken cancellationToken);
    FileStream GetThumbnail(Guid advertisementId);
    void DeleteThumbnail(Guid advertisementId);
}

public class AdvertisementFileStorageDecorator(IFileStorageService fileStorage, IWebHostEnvironment webHostEnvironment)
    : IAdvertisementFileStorageService
{
    private readonly string _basePath = Path.Combine(
        webHostEnvironment.ContentRootPath, 
        "PrivateFiles",
        "advertisements");

    private static readonly Dictionary<string, string> AllowedThumbnailTypes = new()
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp"
    };

    public async Task SaveThumbnailAsync(Stream sourceStream, Guid advertisementId, string fileExtension, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);
        
        if (!AllowedThumbnailTypes.ContainsKey(fileExtension.ToLowerInvariant()))
        {
            throw new InvalidOperationException($"File extension {fileExtension} is not supported for thumbnails");
        }


        if (TryGetThumbnailPath(advertisementId, out string _))
        {
            DeleteThumbnail(advertisementId);
        }
        
        string subdirectoryPath = GetThumbnailFolderPath(advertisementId);
        string fileName = $"thumbnail{fileExtension}";
        
        await fileStorage.SaveFileAsync(sourceStream, subdirectoryPath, fileName, cancellationToken);
    }

    public FileStream GetThumbnail(Guid advertisementId)
    {
        string filePath = GetThumbnailPath(advertisementId);
        return fileStorage.GetFileStream(filePath);
    }

    public void DeleteThumbnail(Guid advertisementId)
    {
        string currentPath = GetThumbnailPath(advertisementId);
        fileStorage.DeleteFile(currentPath);
    }
    
    public static string GetContentType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        return AllowedThumbnailTypes.GetValueOrDefault(extension, "application/octet-stream");
    }
    
    private string GetThumbnailPath(Guid advertisementId)
    {
        string thumbnailDirectory = Path.Combine(_basePath, advertisementId.ToString(), "thumbnail");
        string thumbnail = Directory.GetFiles(thumbnailDirectory)[0];
        return thumbnail;
    }
    
    private bool TryGetThumbnailPath(Guid advertisementId, out string thumbnailPath)
    {
        string thumbnailDirectory = Path.Combine(_basePath, advertisementId.ToString(), "thumbnail");
        string? thumbnail = Directory.GetFiles(thumbnailDirectory).FirstOrDefault();
        thumbnailPath = thumbnail ?? "";
        return thumbnail is not null;
    }
    
    private string GetThumbnailFolderPath(Guid advertisementId) => Path.Combine(_basePath, advertisementId.ToString(), "thumbnail");
}
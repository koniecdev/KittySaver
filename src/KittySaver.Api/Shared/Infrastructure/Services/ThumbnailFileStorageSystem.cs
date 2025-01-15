namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface IThumbnailStorageService
{
    Task SaveThumbnailAsync(IFormFile sourceFile, string entityType, Guid entityId, CancellationToken cancellationToken);
    FileStream GetThumbnail(string entityType, Guid entityId);
    string GetContentType(string fileName);
    
    public static class Constants
    {
        public const int MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
        public static readonly IReadOnlyDictionary<string, string> AllowedThumbnailTypes = new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };
    }
}

public class ThumbnailStorageService(
    IFileStorageService fileStorage,
    IWebHostEnvironment webHostEnvironment)
    : IThumbnailStorageService
{
    private readonly string _basePath = Path.Combine(webHostEnvironment.ContentRootPath, "PrivateFiles");

    public async Task SaveThumbnailAsync(IFormFile sourceFile, string entityType, Guid entityId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        ArgumentException.ThrowIfNullOrEmpty(entityType);
        
        if (sourceFile.Length > IThumbnailStorageService.Constants.MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of 5MB");
        }
        
        string fileExtension = Path.GetExtension(sourceFile.FileName);
        if (!IThumbnailStorageService.Constants.AllowedThumbnailTypes.ContainsKey(fileExtension.ToLowerInvariant()))
        {
            throw new InvalidOperationException($"File extension {fileExtension} is not supported for thumbnails");
        }

        if (TryGetThumbnailPath(entityType, entityId, out string existingPath))
        {
            fileStorage.DeleteFile(existingPath);
        }
        
        string subdirectoryPath = GetThumbnailFolderPath(entityType, entityId);
        string fileName = $"thumbnail{fileExtension}";
        
        await using Stream sourceStream = sourceFile.OpenReadStream();
        await fileStorage.SaveFileAsync(sourceStream, subdirectoryPath, fileName, cancellationToken);
    }

    public FileStream GetThumbnail(string entityType, Guid entityId)
    {
        string thumbnailDirectory = GetThumbnailFolderPath(entityType, entityId);
        
        if (!Directory.Exists(thumbnailDirectory))
        {
            throw new FileNotFoundException($"Thumbnail for {entityType} {entityId} was not found");
        }

        string[] files = Directory.GetFiles(thumbnailDirectory);
        if (files.Length == 0)
        {
            throw new FileNotFoundException($"Thumbnail for {entityType} {entityId} was not found");
        }

        return fileStorage.GetFileStream(files[0]);
    }
    
    public string GetContentType(string fileName)
    {
        string extension = Path.GetExtension(fileName).ToLowerInvariant();
        return IThumbnailStorageService.Constants.AllowedThumbnailTypes
            .GetValueOrDefault(extension, "application/octet-stream");
    }
    
    private bool TryGetThumbnailPath(string entityType, Guid entityId, out string thumbnailPath)
    {
        thumbnailPath = string.Empty;
        string thumbnailDirectory = GetThumbnailFolderPath(entityType, entityId);
        
        if (!Directory.Exists(thumbnailDirectory))
        {
            return false;
        }

        string[] files = Directory.GetFiles(thumbnailDirectory);
        if (files.Length == 0)
        {
            return false;
        }

        thumbnailPath = files[0];
        return true;
    }
    
    private string GetThumbnailFolderPath(string entityType, Guid entityId) => 
        Path.Combine(_basePath, entityType.ToLowerInvariant(), entityId.ToString(), "thumbnail");
}
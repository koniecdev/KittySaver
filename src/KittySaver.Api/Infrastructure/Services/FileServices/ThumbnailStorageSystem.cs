using ImageMagick;

namespace KittySaver.Api.Infrastructure.Services.FileServices;

public interface IThumbnailStorageService<in TId> where TId : struct
{
    Task SaveThumbnailAsync(IFormFile sourceFile, string entityType, TId entityId, CancellationToken cancellationToken);
    FileStream GetThumbnail(string entityType, TId entityId);
    string GetContentType(string fileName);
    void DeleteThumbnail(string entityType, TId entityId);
    
}

public class ThumbnailStorageService<TId>(
    IFileStorageService fileStorage,
    IWebHostEnvironment webHostEnvironment)
    : IThumbnailStorageService<TId> where TId : struct
{
    private readonly string _basePath = Path.Combine(webHostEnvironment.ContentRootPath, "PrivateFiles");

    public async Task SaveThumbnailAsync(IFormFile sourceFile, string entityType, TId entityId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceFile);
        ArgumentException.ThrowIfNullOrEmpty(entityType);
    
        string fileExtension = Path.GetExtension(sourceFile.FileName);
        if (!AllowedPictureTypes.AllowedImageTypes.ContainsKey(fileExtension.ToLowerInvariant()))
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
        using MagickImage image = new(sourceStream);
    
        double ratio = (double)image.Height / image.Width;
        const uint newWidth = 300;
        uint newHeight = (uint)(newWidth * ratio);
    
        image.Resize(width: newWidth, height: newHeight);
    
        using MemoryStream memoryStream = new();
        await image.WriteAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;
    
        await fileStorage.SaveFileAsync(memoryStream, subdirectoryPath, fileName, cancellationToken);
    }

    public FileStream GetThumbnail(string entityType, TId entityId)
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
    
    public string GetContentType(string fileName) => fileStorage.GetContentType(fileName, AllowedPictureTypes.AllowedImageTypes);

    public void DeleteThumbnail(string entityType, TId entityId)
    {
        string thumbnailDirectory = GetThumbnailFolderPath(entityType, entityId);

        if (!Directory.Exists(thumbnailDirectory))
        {
            return;
        }
        
        string[] files = Directory.GetFiles(thumbnailDirectory);
        if (files.Length == 0)
        {
            return;
        }
        
        foreach (string file in files)
        {
            File.Delete(file);
        }
    }

    private bool TryGetThumbnailPath(string entityType, TId entityId, out string thumbnailPath)
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
    
    private string GetThumbnailFolderPath(string entityType, TId entityId) => 
        Path.Combine(_basePath, entityType.ToLowerInvariant(), entityId.ToString()!, "thumbnail");
}
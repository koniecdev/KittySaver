using ImageMagick;
using KittySaver.Domain.Common.Exceptions;

namespace KittySaver.Api.Shared.Infrastructure.Services.FileServices;

public interface IGalleryStorageService
{
    Task<IReadOnlyList<string>> SaveGalleryImagesAsync(IEnumerable<IFormFile> sourceFiles, string entityType, Guid entityId, CancellationToken cancellationToken);
    IReadOnlyList<FileStream> GetGalleryImages(string entityType, Guid entityId);
    FileStream GetGalleryImage(string entityType, Guid entityId, string filename);
    IDictionary<string, string> GetGalleryImagePaths(string entityType, Guid entityId);
    string GetContentType(string fileName);
    void DeleteGalleryImage(string entityType, Guid entityId, string filename);
    void DeleteAllGalleryImages(string entityType, Guid entityId);
    
    public static class Constants
    {
        public static readonly IReadOnlyDictionary<string, string> AllowedImageTypes = new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".jpeg"] = "image/jpeg",
            [".png"] = "image/png",
            [".webp"] = "image/webp"
        };
    }
}

public class GalleryStorageService(
    IFileStorageService fileStorage,
    IWebHostEnvironment webHostEnvironment)
    : IGalleryStorageService
{
    private readonly string _basePath = Path.Combine(webHostEnvironment.ContentRootPath, "PrivateFiles");

    public async Task<IReadOnlyList<string>> SaveGalleryImagesAsync(
        IEnumerable<IFormFile> sourceFiles, 
        string entityType, 
        Guid entityId, 
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceFiles);
        ArgumentException.ThrowIfNullOrEmpty(entityType);

        List<string> savedFilePaths = [];
        
        foreach (IFormFile sourceFile in sourceFiles)
        {
            string fileExtension = Path.GetExtension(sourceFile.FileName);
            
            if (!IGalleryStorageService.Constants.AllowedImageTypes.ContainsKey(fileExtension.ToLowerInvariant()))
            {
                throw new InvalidOperationException($"File extension {fileExtension} is not supported for gallery images");
            }

            string subdirectoryPath = GetGalleryFolderPath(entityType, entityId);
            
            string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            
            await using Stream sourceStream = sourceFile.OpenReadStream();
            
            using MemoryStream processedImageStream = await ProcessGalleryImageAsync(sourceStream, cancellationToken);
            
            await fileStorage.SaveFileAsync(processedImageStream, subdirectoryPath, uniqueFileName, cancellationToken);
            
            savedFilePaths.Add(uniqueFileName);
        }
        
        return savedFilePaths;
    }

    public IReadOnlyList<FileStream> GetGalleryImages(string entityType, Guid entityId)
    {
        string galleryDirectory = GetGalleryFolderPath(entityType, entityId);
        string fullDirectoryPath = Path.Combine(_basePath, galleryDirectory);
        
        if (!Directory.Exists(fullDirectoryPath))
        {
            return new List<FileStream>();
        }

        string[] filePaths = Directory.GetFiles(fullDirectoryPath);
        
        List<FileStream> fileStreams = [];
        foreach (string filePath in filePaths)
        {
            fileStreams.Add(fileStorage.GetFileStream(filePath));
        }
        
        return fileStreams;
    }

    public FileStream GetGalleryImage(string entityType, Guid entityId, string filename)
    {
        string galleryDirectory = GetGalleryFolderPath(entityType, entityId);
        string fullPath = Path.Combine(_basePath, galleryDirectory, filename);
        
        if (!File.Exists(fullPath))
        {
            throw new NotFoundExceptions.FileNotFoundException(filename);
        }
        
        return fileStorage.GetFileStream(fullPath);
    }

    public IDictionary<string, string> GetGalleryImagePaths(string entityType, Guid entityId)
    {
        string galleryDirectory = GetGalleryFolderPath(entityType, entityId);
        string fullDirectoryPath = Path.Combine(_basePath, galleryDirectory);
        
        if (!Directory.Exists(fullDirectoryPath))
        {
            return new Dictionary<string, string>();
        }

        string[] filePaths = Directory.GetFiles(fullDirectoryPath);
        
        Dictionary<string, string> result = new();
        foreach (string filePath in filePaths)
        {
            string filename = Path.GetFileName(filePath);
            result[filename] = filePath;
        }
        
        return result;
    }

    public string GetContentType(string fileName) => fileStorage.GetContentType(fileName, IGalleryStorageService.Constants.AllowedImageTypes);

    public void DeleteGalleryImage(string entityType, Guid entityId, string filename)
    {
        string galleryDirectory = GetGalleryFolderPath(entityType, entityId);
        string fullPath = Path.Combine(_basePath, galleryDirectory, filename);

        if (!File.Exists(fullPath))
        {
            throw new NotFoundExceptions.FileNotFoundException(filename);
        }
        
        File.Delete(fullPath);
    }

    public void DeleteAllGalleryImages(string entityType, Guid entityId)
    {
        string galleryDirectory = GetGalleryFolderPath(entityType, entityId);
        string fullDirectoryPath = Path.Combine(_basePath, galleryDirectory);
        
        if (!Directory.Exists(fullDirectoryPath))
        {
            return;
        }
        
        string[] filePaths = Directory.GetFiles(fullDirectoryPath);
        foreach (string filePath in filePaths)
        {
            File.Delete(filePath);
        }
    }

    private static async Task<MemoryStream> ProcessGalleryImageAsync(Stream sourceStream, CancellationToken cancellationToken)
    {
        using MagickImage image = new(sourceStream);
        
        const int maxDimension = 1200;
        if (image.Width > maxDimension || image.Height > maxDimension)
        {
            if (image.Width > image.Height)
            {
                image.Resize(maxDimension, 0);
            }
            else
            {
                image.Resize(0, maxDimension);
            }
        }
        
        image.Quality = 85;
        
        MemoryStream resultStream = new();
        await image.WriteAsync(resultStream, cancellationToken);
        resultStream.Position = 0;
        
        return resultStream;
    }
    
    private static string GetGalleryFolderPath(string entityType, Guid entityId) => 
        Path.Combine(entityType.ToLowerInvariant(), entityId.ToString(), "gallery");
}
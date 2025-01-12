namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface IFileStorageService
{
    Task SaveFileAsync(Stream sourceStream, string fileName, CancellationToken cancellationToken);
    Task<FileStream> GetFileAsync(string fileName, CancellationToken cancellationToken);
    Task DeleteFileAsync(string fileName, CancellationToken cancellationToken);
}

public sealed class LocalFileStorageService(IWebHostEnvironment webHostEnvironment) : IFileStorageService
{
    private readonly string _basePath = Path.Combine(
        webHostEnvironment.ContentRootPath, 
        "PrivateFiles", 
        "advertisements");

    public async Task SaveFileAsync(Stream sourceStream, string fileName, CancellationToken cancellationToken)
    {
        string advertisementId = Path.GetFileNameWithoutExtension(fileName).Split('_')[0];
        string thumbnailDirectory = Path.Combine(_basePath, advertisementId, "thumbnail");
        
        // Create directory if it doesn't exist
        Directory.CreateDirectory(thumbnailDirectory);
        
        // Clean existing thumbnail (if any)
        foreach (string existingFile in Directory.GetFiles(thumbnailDirectory))
        {
            File.Delete(existingFile);
        }
        
        // Save new thumbnail
        string extension = Path.GetExtension(fileName);
        string filePath = Path.Combine(thumbnailDirectory, $"thumbnail{extension}");
        
        await using FileStream destinationStream = File.Create(filePath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
    }

    public Task<FileStream> GetFileAsync(string fileName, CancellationToken cancellationToken)
    {
        string advertisementId = Path.GetFileNameWithoutExtension(fileName).Split('_')[0];
        string thumbnailDirectory = Path.Combine(_basePath, advertisementId, "thumbnail");
        
        if (!Directory.Exists(thumbnailDirectory))
        {
            throw new FileNotFoundException($"Thumbnail for advertisement {advertisementId} not found");
        }

        string[] files = Directory.GetFiles(thumbnailDirectory);
        if (files.Length == 0)
        {
            throw new FileNotFoundException($"Thumbnail for advertisement {advertisementId} not found");
        }

        string thumbnailPath = files[0];
        return Task.FromResult(new FileStream(thumbnailPath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteFileAsync(string fileName, CancellationToken cancellationToken)
    {
        string advertisementId = Path.GetFileNameWithoutExtension(fileName).Split('-')[0];
        string thumbnailDirectory = Path.Combine(_basePath, advertisementId, "thumbnail");

        if (!Directory.Exists(thumbnailDirectory))
        {
            return Task.CompletedTask;
        }
        Directory.Delete(thumbnailDirectory, recursive: true);
            
        string advertisementDirectory = Path.GetDirectoryName(thumbnailDirectory)!;
        if (!Directory.EnumerateFileSystemEntries(advertisementDirectory).Any())
        {
            Directory.Delete(advertisementDirectory);
        }

        return Task.CompletedTask;
    }
}
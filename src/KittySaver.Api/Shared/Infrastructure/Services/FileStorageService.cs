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
        string advertisementDirectory = Path.Combine(_basePath, advertisementId);
        
        Directory.CreateDirectory(advertisementDirectory);
        
        string extension = Path.GetExtension(fileName);
        string newFileName = $"thumbnail{extension}";
        string filePath = Path.Combine(advertisementDirectory, newFileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
        await using FileStream destinationStream = File.Create(filePath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
    }

    public Task<FileStream> GetFileAsync(string fileName, CancellationToken cancellationToken)
    {
        string advertisementId = Path.GetFileNameWithoutExtension(fileName).Split('-')[0];
        string extension = Path.GetExtension(fileName);
        
        string filePath = Path.Combine(_basePath, advertisementId, $"thumbnail{extension}");
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Thumbnail for advertisement {advertisementId} not found");
        }
        
        return Task.FromResult(new FileStream(filePath, FileMode.Open, FileAccess.Read));
    }

    public Task DeleteFileAsync(string fileName, CancellationToken cancellationToken)
    {
        string advertisementId = Path.GetFileNameWithoutExtension(fileName).Split('-')[0];
        string extension = Path.GetExtension(fileName);
        string filePath = Path.Combine(_basePath, advertisementId, $"thumbnail{extension}");
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            
            // Optionally clean up empty directory
            string directory = Path.GetDirectoryName(filePath)!;
            if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }
        
        return Task.CompletedTask;
    }
}
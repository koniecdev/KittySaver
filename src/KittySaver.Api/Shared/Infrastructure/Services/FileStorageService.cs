namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface IFileStorageService
{
    Task SaveFileAsync(Stream sourceStream, string? subdirectoryPath, string filenameWithExtension, CancellationToken cancellationToken);
    public FileStream GetFileStream(string filePath);
    public FileStream GetFileSteam(string subdirectoryPath, string fileName);
    void DeleteFile(string path);
}

public sealed class LocalFileStorageService(IWebHostEnvironment webHostEnvironment) : IFileStorageService
{
    private readonly string _basePath = Path.Combine(
        webHostEnvironment.ContentRootPath, 
        "PrivateFiles");

    public async Task SaveFileAsync(Stream sourceStream, string? subdirectoryPath, string filenameWithExtension, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(sourceStream);
        ArgumentException.ThrowIfNullOrEmpty(subdirectoryPath);
        ArgumentException.ThrowIfNullOrEmpty(filenameWithExtension);

        string fullDirectoryPath = Path.Combine(_basePath, subdirectoryPath);
        if (!Directory.Exists(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }

        string fullPath = Path.Combine(fullDirectoryPath, filenameWithExtension);

        await using FileStream destinationStream = File.Create(fullPath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
    }

    private string[] GetAllFilesPaths(string subdirectoryPath)
    {
        string directory = Path.Combine(_basePath, subdirectoryPath);
        if (!Directory.Exists(directory))
        {
            throw new FileNotFoundException($"Required path ${subdirectoryPath} has not been found.");
        }
        string[] files = Directory.GetFiles(directory);
        return files;
    }
    
    public FileStream GetFileStream(string filePath) => new(filePath, FileMode.Open, FileAccess.Read);

    public FileStream GetFileSteam(string subdirectoryPath, string fileName)
    {
        string fullPath = Path.Combine(_basePath, subdirectoryPath);

        string[] files = GetAllFilesPaths(fullPath);
        string? filePath = files.FirstOrDefault(x => x == fileName || x.Contains(fileName));
        if (filePath is null)
        {
            throw new FileNotFoundException($"File with name that is equal or starts with {fileName} within {subdirectoryPath} has not been found");
        }

        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }
    
    public void DeleteFile(string filePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(filePath);

        string fullPath = Path.Combine(_basePath, filePath);
        
        if (!File.Exists(fullPath))
        {
            return;
        }

        File.Delete(fullPath);
    }
}
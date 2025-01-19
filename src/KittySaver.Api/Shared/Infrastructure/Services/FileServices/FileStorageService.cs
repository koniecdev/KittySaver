namespace KittySaver.Api.Shared.Infrastructure.Services.FileServices;

public interface IFileStorageService
{
    Task SaveFileAsync(Stream sourceStream, string? subdirectoryPath, string filenameWithExtension, CancellationToken cancellationToken);
    FileStream GetFileStream(string filePath);
    FileStream GetFileStream(string subdirectoryPath, string fileName);
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
        Directory.CreateDirectory(fullDirectoryPath); // CreateDirectory is safe to call if directory exists

        string fullPath = Path.Combine(fullDirectoryPath, filenameWithExtension);
        await using FileStream destinationStream = File.Create(fullPath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
    }

    public FileStream GetFileStream(string filePath) => 
        new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

    public FileStream GetFileStream(string subdirectoryPath, string fileName)
    {
        string fullPath = Path.Combine(_basePath, subdirectoryPath);
        string[] files = GetAllFilesPaths(fullPath);
        
        string? filePath = files.FirstOrDefault(x => x == fileName || x.Contains(fileName));
        if (filePath is null)
        {
            throw new FileNotFoundException($"File with name that is equal or starts with '{fileName}' within '{subdirectoryPath}' was not found");
        }

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
    
    private static string[] GetAllFilesPaths(string directory)
    {
        if (!Directory.Exists(directory))
        {
            throw new DirectoryNotFoundException($"Required directory '{directory}' was not found.");
        }
        return Directory.GetFiles(directory);
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
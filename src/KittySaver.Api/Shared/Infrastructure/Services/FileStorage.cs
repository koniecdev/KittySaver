namespace KittySaver.Api.Shared.Infrastructure.Services;

public interface IFileStorage
{
    Task SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken);
}

public class LocalFileStorage(IWebHostEnvironment webHostEnvironment) : IFileStorage
{
    public async Task SaveFileAsync(Stream sourceStream, string fileName, CancellationToken cancellationToken)
    {
        string fileId = Path.GetFileNameWithoutExtension(fileName).Split('_')[0];
        
        string advertisementsPath = Path.Combine(webHostEnvironment.WebRootPath, "advertisements");
        string advertisementDirectory = Path.Combine(advertisementsPath, fileId);
        
        Directory.CreateDirectory(advertisementDirectory); // CreateDirectory is safe to call even if directory exists
        
        string extension = Path.GetExtension(fileName);
        string newFileName = $"thumbnail{extension}";
        
        string filePath = Path.Combine(advertisementDirectory, newFileName);
        
        await using FileStream destinationStream = File.Create(filePath);
        await sourceStream.CopyToAsync(destinationStream, cancellationToken);
    }
}
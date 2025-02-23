using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;

namespace KittySaver.Wasm.Shared.Validation;

public interface IFileValidationService
{
    ValidationResult? ValidateImageFile(IBrowserFile? file, bool isRequired = true);
}

public class FileValidationService : IFileValidationService
{
    public ValidationResult? ValidateImageFile(IBrowserFile? file, bool isRequired = true)
    {
        if (file == null)
        {
            return isRequired 
                ? new ValidationResult("Zdjęcie jest wymagane") 
                : null;
        }

        string extension = Path.GetExtension(file.Name).ToLowerInvariant();
        
        if (!ImagesValidationConstants.AllowedImageExtensions.Contains(extension))
        {
            return new ValidationResult($"Dozwolone tylko pliki: {string.Join(", ", ImagesValidationConstants.AllowedImageExtensions)}");
        }

        if (!ImagesValidationConstants.AllowedMimeTypes.TryGetValue(extension, out string? expectedMimeType) 
            || file.ContentType != expectedMimeType)
        {
            return new ValidationResult("Nieprawidłowy typ pliku");
        }

        return null;
    }
}
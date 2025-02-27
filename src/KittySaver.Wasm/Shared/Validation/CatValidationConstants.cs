namespace KittySaver.Wasm.Shared.Validation;


public static class CatValidationConstants
{
    public const int NameMaxLength = 100;
    public const int AdditionalRequirementsMaxLength = 2000;
}

public static class ImagesValidationConstants
{
    public static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    public static readonly Dictionary<string, string> AllowedMimeTypes = new()
    {
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".png", "image/png"},
        {".webp", "image/webp"}
    };
}

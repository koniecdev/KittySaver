namespace KittySaver.Shared.Common;

public static class AllowedPictureTypes
{
    public static readonly IReadOnlyDictionary<string, string> AllowedImageTypes = new Dictionary<string, string>
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".webp"] = "image/webp"
    };
}
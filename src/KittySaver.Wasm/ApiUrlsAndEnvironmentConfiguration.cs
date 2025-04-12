namespace KittySaver.Wasm;

public static class EnvironmentConfiguration
{
    private const bool IsDev = true;
    public const string AuthUrl = IsDev ? "https://localhost:44371/api/v1/" : "https://auth.uratujkota.pl/api/v1/";
    public const string ApiUrl = IsDev ? "https://localhost:7127/api/v1/" : "https://api.uratujkota.pl/api/v1/";
}
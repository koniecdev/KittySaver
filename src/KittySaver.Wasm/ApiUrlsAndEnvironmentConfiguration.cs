namespace KittySaver.Wasm;

public static class EnvironmentConfiguration
{
    public static string AuthUrl { get; private set; } = "https://localhost:44371/api/v1/";
    public static string ApiUrl { get; private set; } = "https://localhost:7127/api/v1/";

    public static void Initialize(IApiUrlProvider apiUrlProvider)
    {
        AuthUrl = apiUrlProvider.AuthUrl;
        ApiUrl = apiUrlProvider.ApiUrl;
    }
}

public interface IApiUrlProvider
{
    string AuthUrl { get; }
    string ApiUrl { get; }
}

public class ApiUrlProvider(IConfiguration configuration) : IApiUrlProvider
{
    public string AuthUrl { get; } = configuration["ApiUrls:AuthUrl"] ?? "https://localhost:44371/api/v1/";
    public string ApiUrl { get; } = configuration["ApiUrls:ApiUrl"] ?? "https://localhost:7127/api/v1/";
}
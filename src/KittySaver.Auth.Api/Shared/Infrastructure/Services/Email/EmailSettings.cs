namespace KittySaver.Auth.Api.Shared.Infrastructure.Services.Email;

public class EmailSettings
{
    public string Server { get; init; } = string.Empty;
    public int Port { get; init; }
    public string SenderEmail { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool UseSsl { get; init; }
    public string WebsiteBaseUrl { get; init; } = string.Empty;
}
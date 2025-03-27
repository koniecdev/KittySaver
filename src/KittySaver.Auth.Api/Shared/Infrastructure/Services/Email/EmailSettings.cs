namespace KittySaver.Auth.Api.Shared.Infrastructure.Services.Email;

public class EmailSettings
{
    public string Server { get; set; } = string.Empty;
    public int Port { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
    public string WebsiteBaseUrl { get; set; } = string.Empty;
}
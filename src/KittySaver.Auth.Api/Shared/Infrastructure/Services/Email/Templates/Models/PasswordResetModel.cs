namespace KittySaver.Auth.Api.Shared.Infrastructure.Services.Email.Templates;

public class PasswordResetModel
{
    public string ResetLink { get; init; } = string.Empty;
    public string WebsiteBaseUrl { get; init; } = string.Empty;
}
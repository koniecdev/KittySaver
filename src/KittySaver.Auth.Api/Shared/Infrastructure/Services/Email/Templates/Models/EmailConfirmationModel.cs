namespace KittySaver.Auth.Api.Shared.Infrastructure.Services.Email.Templates;

public class EmailConfirmationModel
{
    public string ConfirmationLink { get; init; } = string.Empty;
    public string WebsiteBaseUrl { get; init; } = string.Empty;
}
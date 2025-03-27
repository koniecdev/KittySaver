using System.Net;
using System.Net.Mail;
using FluentEmail.Core;
using Microsoft.Extensions.Options;

namespace KittySaver.Auth.Api.Shared.Infrastructure.Services.Email;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
    Task SendPasswordResetAsync(string email, string resetLink);
}

public class FluentEmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly IFluentEmail _fluentEmail;
    private readonly ILogger<FluentEmailService> _logger;

    public FluentEmailService(
        IOptions<EmailSettings> emailSettings,
        IFluentEmail fluentEmail,
        ILogger<FluentEmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _fluentEmail = fluentEmail;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var result = await _fluentEmail
                .To(email)
                .Subject(subject)
                .Body(htmlMessage, true)
                .SendAsync();
                
            if (result.Successful)
            {
                _logger.LogInformation("Email sent successfully to {Email}", email);
            }
            else
            {
                _logger.LogError("Failed to send email to {Email}. Errors: {Errors}", 
                    email, string.Join(", ", result.ErrorMessages));
                throw new Exception($"Email could not be sent. Errors: {string.Join(", ", result.ErrorMessages)}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An exception occurred while sending email to {Email}", email);
            throw;
        }
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        var model = new EmailConfirmationModel
        {
            ConfirmationLink = confirmationLink,
            WebsiteBaseUrl = _emailSettings.WebsiteBaseUrl
        };
        
        try
        {
            var result = await _fluentEmail
                .To(email)
                .Subject("Potwierdź swoje konto w KittySaver")
                .UsingTemplateFromFile("./Shared/Infrastructure/Services/Email/Templates/EmailConfirmation.cshtml", model)
                .SendAsync();
                
            if (!result.Successful)
            {
                _logger.LogError("Failed to send confirmation email to {Email}. Errors: {Errors}", 
                    email, string.Join(", ", result.ErrorMessages));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception when sending confirmation email to {Email}", email);
            
            // Jako fallback, jeśli szablon nie zadziała, użyjmy podstawowej wersji
            string subject = "Potwierdź swoje konto w KittySaver";
            string htmlMessage = $@"
            <h1>Witamy w KittySaver!</h1>
            <p>Dziękujemy za rejestrację. Kliknij poniższy link, aby potwierdzić swój adres email:</p>
            <p><a href='{confirmationLink}'>Potwierdź swój email</a></p>
            <p>Jeśli nie rejestrowałeś się w naszym serwisie, możesz zignorować tę wiadomość.</p>
            <p>Pozdrawiamy,<br>Zespół KittySaver</p>";
            
            await SendEmailAsync(email, subject, htmlMessage);
        }
    }

    public async Task SendPasswordResetAsync(string email, string resetLink)
    {
        var model = new PasswordResetModel
        {
            ResetLink = resetLink,
            WebsiteBaseUrl = _emailSettings.WebsiteBaseUrl
        };
        
        try
        {
            var result = await _fluentEmail
                .To(email)
                .Subject("Reset hasła w KittySaver")
                .UsingTemplateFromFile("./Shared/Infrastructure/Services/Email/Templates/PasswordReset.cshtml", model)
                .SendAsync();
                
            if (!result.Successful)
            {
                _logger.LogError("Failed to send password reset email to {Email}. Errors: {Errors}", 
                    email, string.Join(", ", result.ErrorMessages));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception when sending password reset email to {Email}", email);
            
            // Jako fallback, jeśli szablon nie zadziała, użyjmy podstawowej wersji
            string subject = "Reset hasła w KittySaver";
            string htmlMessage = $@"
            <h1>Reset hasła</h1>
            <p>Otrzymaliśmy prośbę o reset hasła dla Twojego konta. Kliknij poniższy link, aby zresetować swoje hasło:</p>
            <p><a href='{resetLink}'>Resetuj hasło</a></p>
            <p>Link wygaśnie po 24 godzinach.</p>
            <p>Jeśli nie prosiłeś o reset hasła, możesz zignorować tę wiadomość.</p>
            <p>Pozdrawiamy,<br>Zespół KittySaver</p>";
            
            await SendEmailAsync(email, subject, htmlMessage);
        }
    }
}

public class EmailConfirmationModel
{
    public string ConfirmationLink { get; set; } = string.Empty;
    public string WebsiteBaseUrl { get; set; } = string.Empty;
}

public class PasswordResetModel
{
    public string ResetLink { get; set; } = string.Empty;
    public string WebsiteBaseUrl { get; set; } = string.Empty;
}
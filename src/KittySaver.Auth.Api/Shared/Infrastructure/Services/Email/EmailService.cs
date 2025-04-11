using FluentEmail.Core;
using FluentEmail.Core.Models;

namespace KittySaver.Auth.Api.Shared.Infrastructure.Services.Email;

public interface IEmailService
{
    Task SendEmailConfirmationAsync(string email, string confirmationLink);
    Task SendPasswordResetAsync(string email, string resetLink);
}

public class EmailService(
    IFluentEmail fluentEmail,
    ILogger<EmailService> logger)
    : IEmailService
{
    private async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            SendResponse? result = await fluentEmail
                .To(email)
                .Subject(subject)
                .Body(htmlMessage, true)
                .SendAsync();
                
            if (result.Successful)
            {
                logger.LogInformation("Email sent successfully to {Email}", email);
            }
            else
            {
                logger.LogError("Failed to send email to {Email}. Errors: {Errors}", 
                    email, string.Join(", ", result.ErrorMessages));
                throw new Exception($"Email could not be sent. Errors: {string.Join(", ", result.ErrorMessages)}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while sending email to {Email}", email);
            throw;
        }
    }

    public async Task SendEmailConfirmationAsync(string email, string confirmationLink)
    {
        const string subject = "Potwierdź swoje konto w KittySaver";
        string htmlMessage = $"""
                              
                                      <h1>Witamy w KittySaver!</h1>
                                      <p>Dziękujemy za rejestrację. Kliknij poniższy link, aby potwierdzić swój adres email:</p>
                                      <p><a href='{confirmationLink}'>Potwierdź swój email</a></p>
                                      <p>Jeśli nie rejestrowałeś się w naszym serwisie, możesz zignorować tę wiadomość.</p>
                                      <p>Pozdrawiamy,<br>Zespół KittySaver</p>
                              """;
        
        await SendEmailAsync(email, subject, htmlMessage);
    }

    public async Task SendPasswordResetAsync(string email, string resetLink)
    {
        const string subject = "Reset hasła w KittySaver";
        string htmlMessage = $"""
                              
                                      <h1>Reset hasła</h1>
                                      <p>Otrzymaliśmy prośbę o reset hasła dla Twojego konta. Kliknij poniższy link, aby zresetować swoje hasło:</p>
                                      <p><a href='{resetLink}'>Resetuj hasło</a></p>
                                      <p>Link wygaśnie po 24 godzinach.</p>
                                      <p>Jeśli nie prosiłeś o reset hasła, możesz zignorować tę wiadomość.</p>
                                      <p>Pozdrawiamy,<br>Zespół uratujkota.pl</p>
                              """;
        
        await SendEmailAsync(email, subject, htmlMessage);
    }
}
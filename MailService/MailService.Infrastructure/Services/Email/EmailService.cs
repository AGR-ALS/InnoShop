using MailKit.Net.Smtp;
using MailService.Application.Abstractions.Mail;
using MailService.Application.Settings.Email;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MailService.Infrastructure.Services.Email;

public class EmailService : IMailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }
    public async Task SendMailAsync(string email, string subject, string message, CancellationToken cancellationToken)
    {
        using var emailMessage = new MimeMessage();
        
        emailMessage.From.Add(new MailboxAddress(_emailSettings.FromName, _emailSettings.FromEmail));
        emailMessage.To.Add(new MailboxAddress(_emailSettings.ToName, email));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = message };

        using var client = new SmtpClient();
        await client.ConnectAsync(_emailSettings.ClientHost, _emailSettings.ClientPort, _emailSettings.UseSsl, cancellationToken);
        await client.AuthenticateAsync(_emailSettings.ClientLogin, _emailSettings.ClientPassword, cancellationToken);
        await client.SendAsync(emailMessage, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
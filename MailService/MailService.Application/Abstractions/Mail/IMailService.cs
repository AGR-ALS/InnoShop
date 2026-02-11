namespace MailService.Application.Abstractions.Mail;

public interface IMailService
{
    Task SendMailAsync(string email, string subject, string message, CancellationToken cancellationToken);
}
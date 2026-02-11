using MassTransit;
using SharedResources.MessageContracts;
using UserService.Application.Abstractions.MessageEvents;

namespace UserService.Infrastructure.MessageEvents.Publishers;

public class MailEventPublisher : IMailEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MailEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public Task SendMail(string email, string subject, string body)
    {
        return _publishEndpoint.Publish(
            new MailSendingEvent(email, subject, body));
    }
}
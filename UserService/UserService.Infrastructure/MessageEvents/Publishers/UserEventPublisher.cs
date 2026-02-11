using MassTransit;
using SharedResources.MessageContracts;
using UserService.Application.Abstractions.MessageEvents;

namespace UserService.Infrastructure.MessageEvents.Publishers;

public class UserEventPublisher : IUserEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    public UserEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }
    
    public Task UserActivationChanged(Guid userId, bool isActive)
    {
        return _publishEndpoint.Publish(
            new UserActivationChanged(userId, isActive)
        );
    }
}
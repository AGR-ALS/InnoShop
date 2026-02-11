namespace UserService.Application.Abstractions.MessageEvents;

public interface IUserEventPublisher
{
    Task UserActivationChanged(Guid userId, bool isActive);
}

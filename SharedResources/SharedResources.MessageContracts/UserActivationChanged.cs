namespace SharedResources.MessageContracts;

public record UserActivationChanged(Guid UserId, bool IsActive);
using MassTransit;
using ProductService.Application.Abstractions.Services;
using SharedResources.MessageContracts;

namespace ProductService.Infrastructure.MessageEvents.Consumers;

public class UserActivationChangedConsumer : IConsumer<UserActivationChanged>
{
    private readonly IProductService _productService;

    public UserActivationChangedConsumer(IProductService productService)
    {
        _productService = productService;
    }

    public async Task Consume(ConsumeContext<UserActivationChanged> context)
    {
        var message = context.Message;
        await _productService.SetProductOwnerActiveAsync(
            message.UserId,
            message.IsActive,
            context.CancellationToken
        );
    }
}
namespace UserService.Infrastructure.MessageEvents.Settings;

public class RabbitMqSettings
{
    public string Host { get; set; } = null!;
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
}
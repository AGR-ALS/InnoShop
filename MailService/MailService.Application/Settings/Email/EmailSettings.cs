namespace MailService.Application.Settings.Email;

public class EmailSettings
{
    public string FromName { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string ToName { get; set; } = string.Empty;
    public string ClientHost { get; set; } = string.Empty;
    public int ClientPort { get; set; }
    public string ClientLogin { get; set; } = string.Empty;
    public string ClientPassword { get; set; } = string.Empty;
    public bool UseSsl { get; set; }
}
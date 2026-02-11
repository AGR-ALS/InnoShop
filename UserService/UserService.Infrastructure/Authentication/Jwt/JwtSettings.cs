namespace UserService.Infrastructure.Authentication.Jwt;

public class JwtSettings
{
    public string SecretKey { get; set; } = String.Empty;
    public int ExpiresInMinutes { get; set; }
    public string Issuer { get; set; } = String.Empty;
    public string Audience { get; set; } = String.Empty;
}
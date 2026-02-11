namespace UserService.DataAccess.Entities;

public class SecureTokenEntity
{
    public Guid Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public UserEntity User { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
}
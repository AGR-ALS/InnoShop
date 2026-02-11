namespace UserService.Domain.Models;

public class RefreshToken : SecureToken
{
    
    private RefreshToken(Guid id, string token, Guid userId, DateTime expiresAt)
    {
        Id = id;
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
    }

    public static RefreshToken CreateInstance(string token, Guid userId, DateTime expiresAt)
    {
        return new RefreshToken(Guid.NewGuid(), token, userId, expiresAt);
    }
    
}
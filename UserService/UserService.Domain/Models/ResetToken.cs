namespace UserService.Domain.Models;


public class ResetToken : SecureToken
{
    private ResetToken(Guid id, string token, Guid userId, DateTime expiresAt)
    {
        Id = id;
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
    }

    public static ResetToken CreateInstance(string token, Guid userId, DateTime expiresAt)
    {
        return new ResetToken(Guid.NewGuid(), token, userId, expiresAt);
    }
    
}
namespace UserService.Domain.Models;

public class AccountConfirmationToken : SecureToken
{
    private AccountConfirmationToken(Guid id, string token, Guid userId, DateTime expiresAt)
    {
        Id = id;
        Token = token;
        UserId = userId;
        ExpiresAt = expiresAt;
    }

    public static AccountConfirmationToken CreateInstance(string token, Guid userId, DateTime expiresAt)
    {
        return new AccountConfirmationToken(Guid.NewGuid(), token, userId, expiresAt);
    }
}
using UserService.Application.Abstractions.Authentication;

namespace UserService.Infrastructure.Authentication.Jwt;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.EnhancedHashPassword(password);    
    }

    public bool VerifyHashedPassword(string providedPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.EnhancedVerify(providedPassword, hashedPassword);
    }
}
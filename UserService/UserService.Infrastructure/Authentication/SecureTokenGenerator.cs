using System.Security.Cryptography;
using UserService.Application.Abstractions.Authentication;
using UserService.Domain.Models;

namespace UserService.Infrastructure.Authentication;

public class SecureTokenGenerator : ISecureTokenGenerator
{
    public string GenerateToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    public bool VerifyToken(SecureToken token)
    {
        return token.ExpiresAt >= DateTime.UtcNow;
    }
}
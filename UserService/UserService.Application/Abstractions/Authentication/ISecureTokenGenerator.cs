using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Authentication;

public interface ISecureTokenGenerator
{
    string GenerateToken();
    bool VerifyToken(SecureToken token);
}
using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Services;

public interface ISecureTokenService<T> where T: SecureToken
{
    Task<string> CreateSecureTokenAsync(Guid userId, CancellationToken cancellationToken);
    Task DeleteSecureTokenAsync(string token, CancellationToken cancellationToken);
    Task<bool> ValidateSecureTokenAsync(string token, CancellationToken cancellationToken);
    Task <T> GetSecureTokenModelAsync(string token, CancellationToken cancellationToken);
}
using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Services;

public interface IRefreshTokensService : ISecureTokenService<RefreshToken>
{
}
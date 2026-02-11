using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Repositories;

public interface IRefreshTokensRepository : ISecureTokenRepository<RefreshToken>
{
}
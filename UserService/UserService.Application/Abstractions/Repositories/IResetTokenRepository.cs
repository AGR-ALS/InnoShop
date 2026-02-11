using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Repositories;

public interface IResetTokenRepository : ISecureTokenRepository<ResetToken>
{
}
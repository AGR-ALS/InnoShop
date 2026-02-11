using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Services;

public interface IResetTokenService : ISecureTokenService<ResetToken>
{
}
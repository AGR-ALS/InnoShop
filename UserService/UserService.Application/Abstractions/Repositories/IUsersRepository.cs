using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Repositories;

public interface IUsersRepository
{
    Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken);
    Task<User> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task CreateUserAsync(User user, CancellationToken cancellationToken);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken);
    Task ResetPasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken);
    Task SetUserConfirmedAsync(Guid id, bool isConfirmed, CancellationToken cancellationToken);
    Task SetUserActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
}
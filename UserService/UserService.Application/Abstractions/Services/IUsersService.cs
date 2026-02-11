using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Services;

public interface IUsersService
{
    Task RegisterUserAsync(string username, string email, string password, CancellationToken cancellationToken);
    Task<(string, string)> LoginAsync(string email, string password, CancellationToken cancellationToken);
    Task<string> LoginAsync(string refreshToken, CancellationToken cancellationToken);
    
    Task<List<User>> GetUsersAsync(CancellationToken cancellationToken);
    Task<User> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task UpdateUserAsync(User user, CancellationToken cancellationToken);
    Task DeleteUserAsync(Guid id, CancellationToken cancellationToken);
    Task SendResetPasswordRequestAsync(string email, CancellationToken cancellationToken);
    Task SetNewPasswordAsync(string token, string password, CancellationToken cancellationToken);
    Task SendAccountConfirmationEmail(string email, CancellationToken cancellationToken);
    Task SetAccountAsConfirmed(string token, CancellationToken cancellationToken);
    Task SetAccountActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken);
}
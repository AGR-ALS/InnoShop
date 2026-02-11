using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Repositories;

public interface IRolesRepository
{
    Task<List<Role>> GetAllRolesAsync(CancellationToken cancellationToken);
    Task<Role> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken);
    Task<Role> GetRoleByNameAsync(string name, CancellationToken cancellationToken);
    Task<Guid> CreateRoleAsync(Role role, CancellationToken cancellationToken);
    Task UpdateRoleAsync(Role role, CancellationToken cancellationToken);
    Task DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken);
}
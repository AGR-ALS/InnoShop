using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Domain.Models;

namespace UserService.Application.Services;

public class RolesService : IRolesService
{
    private readonly IRolesRepository _rolesRepository;

    public RolesService(IRolesRepository rolesRepository)
    {
        _rolesRepository = rolesRepository;
    }
    public async Task<List<Role>> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        return await _rolesRepository.GetAllRolesAsync(cancellationToken);
    }
    
    public async Task<Role> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        return await _rolesRepository.GetRoleByIdAsync(roleId, cancellationToken);
    }

    public async Task<Role> GetRoleByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await _rolesRepository.GetRoleByNameAsync(name, cancellationToken);
    }

    public async Task<Guid> CreateRoleAsync(Role role, CancellationToken cancellationToken)
    {
        return await _rolesRepository.CreateRoleAsync(role, cancellationToken);
    }

    public async Task UpdateRoleAsync(Role role, CancellationToken cancellationToken)
    {
        await _rolesRepository.UpdateRoleAsync(role, cancellationToken);
    }

    public async Task DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        await _rolesRepository.DeleteRoleAsync(roleId, cancellationToken);
    }
}
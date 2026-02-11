using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Exceptions;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess.Repositories;

public class RolesRepository : IRolesRepository
{
    private readonly UserServiceDbContext _dbContext;
    private readonly IMapper _mapper;

    public RolesRepository(UserServiceDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<List<Role>> GetAllRolesAsync(CancellationToken cancellationToken)
    {
        var roleEntities = await _dbContext.Roles.AsNoTracking().ToListAsync(cancellationToken: cancellationToken);
        return _mapper.Map<List<Role>>(roleEntities);
    }

    public async Task<Role> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var roleEntity = await _dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == roleId, cancellationToken: cancellationToken);
        if(roleEntity == null)
            throw new NotFoundException("Role was not found");
        return _mapper.Map<Role>(roleEntity);
    }

    public async Task<Role> GetRoleByNameAsync(string name, CancellationToken cancellationToken)
    {
        var roleEntity = await _dbContext.Roles.AsNoTracking().FirstOrDefaultAsync(p => p.Name == name, cancellationToken: cancellationToken);
        if(roleEntity == null)
            throw new NotFoundException("Role was not found");
        return _mapper.Map<Role>(roleEntity);
    }

    public async Task<Guid> CreateRoleAsync(Role role, CancellationToken cancellationToken)
    {
        var roleEntity = _mapper.Map<RoleEntity>(role);
        await _dbContext.Roles.AddAsync(roleEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return roleEntity.Id;
    }

    public async Task UpdateRoleAsync(Role role, CancellationToken cancellationToken)
    {
        var rowsUpdated = await _dbContext.Roles.Where(r=>r.Id == role.Id).ExecuteUpdateAsync(s=>s.
            SetProperty(p => p.Name, p => role.Name), cancellationToken: cancellationToken);
        if (rowsUpdated == 0)
            throw new NotFoundException("Role was not found");
    }

    public async Task DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var rowsDeleted = await _dbContext.Roles.Where(r => r.Id == roleId).ExecuteDeleteAsync(cancellationToken: cancellationToken);
        if(rowsDeleted == 0)
            throw new NotFoundException("Role was not found");
    }
}
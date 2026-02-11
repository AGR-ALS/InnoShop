using System.Security.Cryptography;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Exceptions;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess.Repositories;

public class UsersRepository : IUsersRepository
{
    private readonly UserServiceDbContext _dbContext;
    private readonly IMapper _mapper;

    public UsersRepository(UserServiceDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken)
    {
        var userEntities = await _dbContext.Users.Include(u=>u.Role).AsNoTracking().ToListAsync(cancellationToken);
        return _mapper.Map<List<User>>(userEntities);
    }

    public async Task<User> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var userEntity = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Id == id, cancellationToken);
        if(userEntity == null)
            throw new NotFoundException("User was not found");
        await _dbContext.Roles.Where(r=>r.Id == userEntity.RoleId).LoadAsync(cancellationToken);
        return _mapper.Map<User>(userEntity);
    }

    public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var userEntity = await _dbContext.Users.FirstOrDefaultAsync(u=>u.Email == email, cancellationToken);
        if(userEntity == null)
            throw new NotFoundException("User was not found");
        await _dbContext.Roles.Where(r=>r.Id == userEntity.RoleId).LoadAsync(cancellationToken);
        return _mapper.Map<User>(userEntity);
    }


    public async Task CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        var userEntity = _mapper.Map<UserEntity>(user);
        await _dbContext.Users.AddAsync(userEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        var rowsUpdated = await _dbContext.Users.Where(u => u.Id == user.Id).ExecuteUpdateAsync(s => s
                .SetProperty(p => p.Email, p => user.Email)
                .SetProperty(p => p.Name, p => user.Name)
                .SetProperty(p => p.RoleId, p => user.RoleId)
                .SetProperty(p => p.IsConfirmed, p => user.IsConfirmed)
                .SetProperty(p => p.IsActive, p => user.IsActive),
            cancellationToken);
        if (rowsUpdated == 0)
        {
            throw new NotFoundException("User was not found");
        }
    }
    
    
    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        var rowsDeleted = await _dbContext.Users.Where(u => u.Id == id).ExecuteDeleteAsync(cancellationToken);
        if (rowsDeleted == 0)
        {
            throw new NotFoundException("User was not found");
        }
    }

    public async Task ResetPasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken)
    {
        var rowsUpdated = await _dbContext.Users.Where(u => u.Id == id).ExecuteUpdateAsync(s => s
                .SetProperty(p => p.PasswordHash, p => passwordHash),
            cancellationToken);
        if (rowsUpdated == 0)
        {
            throw new NotFoundException("User was not found");
        }
    }

    public async Task SetUserConfirmedAsync(Guid id, bool isConfirmed, CancellationToken cancellationToken)
    {
        var rowsUpdated = await _dbContext.Users.Where(u => u.Id == id).ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsConfirmed, p => isConfirmed),
            cancellationToken);
        if (rowsUpdated == 0)
        {
            throw new NotFoundException("User was not found");
        }
    }

    public async Task SetUserActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        var rowsUpdated = await _dbContext.Users.Where(u => u.Id == id).ExecuteUpdateAsync(s => s
                .SetProperty(p => p.IsActive, p => isActive),
            cancellationToken);
        if (rowsUpdated == 0)
        {
            throw new NotFoundException("User was not found");
        }
    }
}
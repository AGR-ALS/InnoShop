using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using UserService.Application.Exceptions;
using UserService.DataAccess;
using UserService.DataAccess.Entities;
using UserService.DataAccess.Mapping;
using UserService.DataAccess.Repositories;
using UserService.Domain.Models;
using Xunit;

namespace UserService.Tests.DataAccessTests;

public class RolesRepositoryTests
{
        private readonly IMapper _mapper;

    public RolesRepositoryTests()
    {
        var config = new MapperConfiguration(cfg => { cfg.AddProfile<UserProfile>(); }, new LoggerFactory());
        _mapper = config.CreateMapper();
    }

    private UserServiceDbContext CreateInMemorySqliteDbContext(string dbName)
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<UserServiceDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new UserServiceDbContext(options, new ConfigurationBuilder().Build());
        context.Database.EnsureCreated();
        
        return context;
    }

    [Fact]
    public async Task GetAllRolesAsync_ReturnsAllRoles()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(GetAllRolesAsync_ReturnsAllRoles));
        
        var roleEntities = new List<RoleEntity>
        {
            new RoleEntity { Id = Guid.NewGuid(), Name = "Admin" },
            new RoleEntity { Id = Guid.NewGuid(), Name = "Regular" }
        };

        context.Roles.AddRange(roleEntities);
        await context.SaveChangesAsync();

        var repository = new RolesRepository(context, _mapper);

        
        var roles = await repository.GetAllRolesAsync(CancellationToken.None);

        
        Assert.NotNull(roles);
        Assert.Equal(2, roles.Count);
    }

    [Fact]
    public async Task GetRoleByIdAsync_ReturnsRole_WhenExists()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(GetRoleByIdAsync_ReturnsRole_WhenExists));
        
        var roleId = Guid.NewGuid();
        var roleEntity = new RoleEntity
        {
            Id = roleId,
            Name = "Admin"
        };

        context.Roles.Add(roleEntity);
        await context.SaveChangesAsync();

        var repository = new RolesRepository(context, _mapper);

        
        var role = await repository.GetRoleByIdAsync(roleId, CancellationToken.None);

        
        Assert.NotNull(role);
        Assert.Equal(roleId, role.Id);
        Assert.Equal("Admin", role.Name);
    }

    [Fact]
    public async Task GetRoleByIdAsync_ThrowsNotFoundException_WhenRoleNotFound()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(GetRoleByIdAsync_ThrowsNotFoundException_WhenRoleNotFound));
        var repository = new RolesRepository(context, _mapper);
        var nonExistentId = Guid.NewGuid();

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.GetRoleByIdAsync(nonExistentId, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetRoleByNameAsync_ReturnsRole_WhenExists()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(GetRoleByNameAsync_ReturnsRole_WhenExists));
        
        var roleEntity = new RoleEntity
        {
            Id = Guid.NewGuid(),
            Name = "Admin"
        };

        context.Roles.Add(roleEntity);
        await context.SaveChangesAsync();

        var repository = new RolesRepository(context, _mapper);

        
        var role = await repository.GetRoleByNameAsync("Admin", CancellationToken.None);

        
        Assert.NotNull(role);
        Assert.Equal("Admin", role.Name);
    }

    [Fact]
    public async Task GetRoleByNameAsync_ThrowsNotFoundException_WhenRoleNotFound()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(GetRoleByNameAsync_ThrowsNotFoundException_WhenRoleNotFound));
        var repository = new RolesRepository(context, _mapper);

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.GetRoleByNameAsync("NonExistentRole", CancellationToken.None)
        );
    }

    [Fact]
    public async Task CreateRoleAsync_AddsRoleToDatabase()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(CreateRoleAsync_AddsRoleToDatabase));
        var repository = new RolesRepository(context, _mapper);
        
        var role = Role.CreateInstance("NewRole");
        role.Id = Guid.NewGuid();

        
        var roleId = await repository.CreateRoleAsync(role, CancellationToken.None);

        
        var roleEntity = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        Assert.NotNull(roleEntity);
        Assert.Equal("NewRole", roleEntity.Name);
        Assert.Equal(roleId, roleEntity.Id);
    }

    [Fact]
    public async Task UpdateRoleAsync_UpdatesRoleName()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(UpdateRoleAsync_UpdatesRoleName));
        
        var roleId = Guid.NewGuid();
        var roleEntity = new RoleEntity
        {
            Id = roleId,
            Name = "OldName"
        };

        context.Roles.Add(roleEntity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new RolesRepository(context, _mapper);

        var updatedRole = Role.CreateInstance("NewName");
        updatedRole.Id = roleId;

        
        await repository.UpdateRoleAsync(updatedRole, CancellationToken.None);

        
        var updatedRoleEntity = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        Assert.NotNull(updatedRoleEntity);
        Assert.Equal("NewName", updatedRoleEntity.Name);
    }

    [Fact]
    public async Task UpdateRoleAsync_ThrowsNotFoundException_WhenRoleNotFound()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(UpdateRoleAsync_ThrowsNotFoundException_WhenRoleNotFound));
        var repository = new RolesRepository(context, _mapper);
        
        var nonExistentRole = Role.CreateInstance("NonExistent");

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.UpdateRoleAsync(nonExistentRole, CancellationToken.None)
        );
    }

    [Fact]
    public async Task DeleteRoleAsync_RemovesRoleFromDatabase()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(DeleteRoleAsync_RemovesRoleFromDatabase));
        
        var roleId = Guid.NewGuid();
        var roleEntity = new RoleEntity
        {
            Id = roleId,
            Name = "ToDelete"
        };

        context.Roles.Add(roleEntity);
        await context.SaveChangesAsync();

        var repository = new RolesRepository(context, _mapper);

        
        await repository.DeleteRoleAsync(roleId, CancellationToken.None);

        
        var deletedRole = await context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        Assert.Null(deletedRole);
    }

    [Fact]
    public async Task DeleteRoleAsync_ThrowsNotFoundException_WhenRoleNotFound()
    {
        
        var context = CreateInMemorySqliteDbContext(nameof(DeleteRoleAsync_ThrowsNotFoundException_WhenRoleNotFound));
        var repository = new RolesRepository(context, _mapper);
        var nonExistentId = Guid.NewGuid();

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.DeleteRoleAsync(nonExistentId, CancellationToken.None)
        );
    }
}
using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using UserService.Application.Exceptions;
using UserService.DataAccess;
using UserService.DataAccess.Entities;
using UserService.DataAccess.Mapping;
using UserService.DataAccess.Repositories;
using UserService.Domain.Models;
using Xunit;

namespace UserService.Tests.DataAccessTests;

public class UsersRepositoryTests
{
    private readonly IMapper _mapper;

    public UsersRepositoryTests()
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
    public async Task GetAllUsersAsync_ReturnsAllUsers()
    {
        var context = CreateInMemorySqliteDbContext(nameof(GetAllUsersAsync_ReturnsAllUsers));

        
        var userEntities = new List<UserEntity>
        {
            new UserEntity
            {
                Id = Guid.NewGuid(), Name = "User1", Email = "user1@test.com", PasswordHash = "hash1", Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Regular"},
                IsConfirmed = true, IsActive = true
            },
            new UserEntity
            {
                Id = Guid.NewGuid(), Name = "User2", Email = "user2@test.com", PasswordHash = "hash2", Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Admin"},
                IsConfirmed = true, IsActive = true
            }
        };

        context.Users.AddRange(userEntities);
        await context.SaveChangesAsync();

        var repository = new UsersRepository(context, _mapper);

        
        var users = await repository.GetAllUsersAsync(CancellationToken.None);
        
        
        Assert.NotNull(users);
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task GetUserByIdAsync_ReturnsUser_WhenExists()
    {
        var context = CreateInMemorySqliteDbContext(nameof(GetUserByIdAsync_ReturnsUser_WhenExists));

        
        var userId = Guid.NewGuid();
        var userEntity = new UserEntity
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedPassword",
            Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Regular"},
            IsConfirmed = true,
            IsActive = true
        };

        context.Users.Add(userEntity);
        await context.SaveChangesAsync();

        var repository = new UsersRepository(context, _mapper);

        
        var user = await repository.GetUserByIdAsync(userId, CancellationToken.None);

        
        Assert.NotNull(user);
        Assert.Equal(userId, user.Id);
        Assert.Equal("Test User", user.Name);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var context = CreateInMemorySqliteDbContext(nameof(GetUserByIdAsync_ThrowsNotFoundException_WhenUserNotFound));
        var repository = new UsersRepository(context, _mapper);
        var nonExistentId = Guid.NewGuid();

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.GetUserByIdAsync(nonExistentId, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetUserByEmailAsync_ReturnsUser_WhenExists()
    {
        var context = CreateInMemorySqliteDbContext(nameof(GetUserByEmailAsync_ReturnsUser_WhenExists));

        
        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashedPassword",
            Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Regular"},
            IsConfirmed = false,
            IsActive = true
        };

        context.Users.Add(userEntity);
        await context.SaveChangesAsync();

        var repository = new UsersRepository(context, _mapper);

        
        var user = await repository.GetUserByEmailAsync("test@example.com", CancellationToken.None);

        
        Assert.NotNull(user);
        Assert.Equal("test@example.com", user.Email);
        Assert.Equal("Test User", user.Name);
        Assert.False(user.IsConfirmed);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task GetUserByEmailAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var context = CreateInMemorySqliteDbContext(nameof(GetUserByEmailAsync_ThrowsNotFoundException_WhenUserNotFound));
        var repository = new UsersRepository(context, _mapper);

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.GetUserByEmailAsync("nonexistent@example.com", CancellationToken.None)
        );
    }

    [Fact]
    public async Task CreateUserAsync_AddsUserToDatabase()
    {
        var context = CreateInMemorySqliteDbContext(nameof(CreateUserAsync_AddsUserToDatabase));
        var repository = new UsersRepository(context, _mapper);

        
        var user = User.CreateInstance("New User", "newuser@example.com", "hashedPassword");
        user.Id = Guid.NewGuid();
        user.Role = Role.CreateInstance("Regular");
        user.IsConfirmed = false;
        user.IsActive = true;

        
        await repository.CreateUserAsync(user, CancellationToken.None);

        
        var userEntity = await context.Users.Include(userEntity => userEntity.Role).FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.NotNull(userEntity);
        Assert.Equal("New User", userEntity.Name);
        Assert.Equal("newuser@example.com", userEntity.Email);
        Assert.Equal("hashedPassword", userEntity.PasswordHash);
        Assert.Equal("Regular", userEntity.Role.Name);
        Assert.False(userEntity.IsConfirmed);
        Assert.True(userEntity.IsActive);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdatesUserProperties()
    {
        var context = CreateInMemorySqliteDbContext(nameof(UpdateUserAsync_UpdatesUserProperties));

        
        var userId = Guid.NewGuid();
        var originalUser = new UserEntity
        {
            Id = userId,
            Name = "Old Name",
            Email = "old@example.com",
            PasswordHash = "hash",
            Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Regular"},
            IsConfirmed = true,
            IsActive = true
        };

        context.Users.Add(originalUser);
        var rolesRepository = new RolesRepository(context, _mapper);
        var role = Role.CreateInstance("Admin");
        await rolesRepository.CreateRoleAsync(role, CancellationToken.None);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new UsersRepository(context, _mapper);

        var updatedUser = new User
        {
            Id = userId,
            Name = "New Name",
            Email = "new@example.com",
            Role = role,
            RoleId = role.Id,
        };

        
        await repository.UpdateUserAsync(updatedUser, CancellationToken.None);

        
        var userEntity = await context.Users.Include(userEntity => userEntity.Role).FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(userEntity);
        Assert.Equal("New Name", userEntity.Name);
        Assert.Equal("new@example.com", userEntity.Email);
        Assert.Equal("Admin", userEntity.Role.Name);
        
        Assert.Equal("hash", userEntity.PasswordHash);
        Assert.False(userEntity.IsConfirmed);
        Assert.False(userEntity.IsActive);
    }

    [Fact]
    public async Task UpdateUserAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var context = CreateInMemorySqliteDbContext(nameof(UpdateUserAsync_ThrowsNotFoundException_WhenUserNotFound));
        var userRepository = new UsersRepository(context, _mapper);
        var rolesRepository = new RolesRepository(context, _mapper);
        var role = Role.CreateInstance("Regular");
        await rolesRepository.CreateRoleAsync(role, CancellationToken.None);
        var nonExistentUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            Email = "test@example.com",
            Role = role,
        };

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => userRepository.UpdateUserAsync(nonExistentUser, CancellationToken.None)
        );
    }

    [Fact]
    public async Task DeleteUserAsync_RemovesUserFromDatabase()
    {
        var context = CreateInMemorySqliteDbContext(nameof(DeleteUserAsync_RemovesUserFromDatabase));

        
        var userId = Guid.NewGuid();
        var userEntity = new UserEntity
        {
            Id = userId,
            Name = "To Delete",
            Email = "delete@example.com",
            PasswordHash = "hash",
            Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Regular"},
            IsConfirmed = true,
            IsActive = true
        };

        context.Users.Add(userEntity);
        await context.SaveChangesAsync();

        var repository = new UsersRepository(context, _mapper);

        
        await repository.DeleteUserAsync(userId, CancellationToken.None);

        
        var deletedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        Assert.Null(deletedUser);
    }

    [Fact]
    public async Task DeleteUserAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var context = CreateInMemorySqliteDbContext(nameof(DeleteUserAsync_ThrowsNotFoundException_WhenUserNotFound));
        var repository = new UsersRepository(context, _mapper);
        var nonExistentId = Guid.NewGuid();

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.DeleteUserAsync(nonExistentId, CancellationToken.None)
        );
    }

    [Fact]
    public async Task ResetPasswordAsync_UpdatesPasswordHash()
    {
        var context = CreateInMemorySqliteDbContext(nameof(ResetPasswordAsync_UpdatesPasswordHash));

        
        var userId = Guid.NewGuid();
        var userEntity = new UserEntity
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "oldHash",
            Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Regular"},
            IsConfirmed = true,
            IsActive = true
        };

        context.Users.Add(userEntity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new UsersRepository(context, _mapper);
        var newPasswordHash = "newHashedPassword";

        
        await repository.ResetPasswordAsync(userId, newPasswordHash, CancellationToken.None);

        
        var updatedUser = await repository.GetUserByIdAsync(userId, CancellationToken.None);
        Assert.NotNull(updatedUser);
        Assert.Equal(newPasswordHash, updatedUser.PasswordHash);
        
        Assert.Equal("Test User", updatedUser.Name);
        Assert.Equal("test@example.com", updatedUser.Email);
    }

    [Fact]
    public async Task ResetPasswordAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var context = CreateInMemorySqliteDbContext(nameof(ResetPasswordAsync_ThrowsNotFoundException_WhenUserNotFound));
        var repository = new UsersRepository(context, _mapper);
        var nonExistentId = Guid.NewGuid();

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.ResetPasswordAsync(nonExistentId, "newHash", CancellationToken.None)
        );
    }

    [Fact]
    public async Task SetUserConfirmedAsync_UpdatesIsConfirmedProperty()
    {
        var context = CreateInMemorySqliteDbContext(nameof(SetUserConfirmedAsync_UpdatesIsConfirmedProperty));

        
        var userId = Guid.NewGuid();
        var userEntity = new UserEntity
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Regular"},
            IsConfirmed = false,
            IsActive = true
        };

        context.Users.Add(userEntity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new UsersRepository(context, _mapper);

        
        await repository.SetUserConfirmedAsync(userId, true, CancellationToken.None);

        
        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(updatedUser);
        Assert.True(updatedUser.IsConfirmed);
    }

    [Fact]
    public async Task SetUserActiveAsync_UpdatesIsActiveProperty()
    {
        var context = CreateInMemorySqliteDbContext(nameof(SetUserActiveAsync_UpdatesIsActiveProperty));

        
        var userId = Guid.NewGuid();
        var userEntity = new UserEntity
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Regular"},
            IsConfirmed = true,
            IsActive = true
        };

        context.Users.Add(userEntity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var repository = new UsersRepository(context, _mapper);

        
        await repository.SetUserActiveAsync(userId, false, CancellationToken.None);

        
        var updatedUser = await context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        Assert.NotNull(updatedUser);
        Assert.False(updatedUser.IsActive);
    }

    [Fact]
    public async Task SetUserActiveAsync_ThrowsNotFoundException_WhenUserNotFound()
    {
        var context = CreateInMemorySqliteDbContext(nameof(SetUserActiveAsync_ThrowsNotFoundException_WhenUserNotFound));
        var repository = new UsersRepository(context, _mapper);
        var nonExistentId = Guid.NewGuid();

        
        await Assert.ThrowsAsync<NotFoundException>(
            () => repository.SetUserActiveAsync(nonExistentId, false, CancellationToken.None)
        );
    }
}
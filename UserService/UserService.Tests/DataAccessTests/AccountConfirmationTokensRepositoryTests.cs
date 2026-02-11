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

public class AccountConfirmationTokensRepositoryTests
{
        private readonly IMapper _mapper;
        private readonly SqliteConnection _connection;
        private readonly UserServiceDbContext _context;
        private readonly UserEntity _testUser;

        public AccountConfirmationTokensRepositoryTests()
        {
            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AccountConfirmationTokenProfile>();
                cfg.AddProfile<UserProfile>();
            }, new LoggerFactory());
            _mapper = config.CreateMapper();

            
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<UserServiceDbContext>()
                .UseSqlite(_connection)
                .Options;
            

            _context = new UserServiceDbContext(options, new ConfigurationBuilder().Build());
            _context.Database.EnsureCreated();
            
            
            _testUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = "test@example.com",
                PasswordHash = "hashedPassword",
                Role = new RoleEntity{Id = Guid.NewGuid(), Name = "Regular"},
                IsConfirmed = true,
                IsActive = true
            };
            
            _context.Users.Add(_testUser);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context?.Dispose();
            _connection?.Dispose();
        }

        private AccountConfirmationTokenRepository CreateRepository()
        {
            return new AccountConfirmationTokenRepository(_context, _mapper);
        }

        [Fact]
        public async Task CreateSecureTokenAsync_AddsTokenToDatabase()
        {
            
            var repository = CreateRepository();
            var token = AccountConfirmationToken.CreateInstance(
                "test-token-123",
                _testUser.Id,
                DateTime.UtcNow.AddMinutes(10)
            );

            
            var tokenString = await repository.CreateSecureTokenAsync(token, CancellationToken.None);

            
            Assert.Equal("test-token-123", tokenString);
            
            var tokenEntity = await _context.AccountConfirmationTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == "test-token-123");
            Assert.NotNull(tokenEntity);
            Assert.Equal(_testUser.Id, tokenEntity.UserId);
            Assert.NotNull(tokenEntity.User);
            Assert.Equal(_testUser.Email, tokenEntity.User.Email);
        }

        [Fact]
        public async Task GetSecureTokenAsync_ReturnsToken_WhenExists()
        {
            
            var repository = CreateRepository();
            var expiresAt = DateTime.UtcNow.AddMinutes(10);
            
            var tokenEntity = new AccountConfirmationTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = "existing-token",
                UserId = _testUser.Id,
                User = _testUser,
                ExpiresAt = expiresAt
            };
            
            _context.AccountConfirmationTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            
            var token = await repository.GetSecureTokenAsync("existing-token", CancellationToken.None);

            
            Assert.NotNull(token);
            Assert.Equal("existing-token", token.Token);
            Assert.Equal(_testUser.Id, token.UserId);
            Assert.Equal(expiresAt, token.ExpiresAt);
        }

        [Fact]
        public async Task GetSecureTokenAsync_ThrowsNotFoundException_WhenTokenNotFound()
        {
            
            var repository = CreateRepository();

            
            await Assert.ThrowsAsync<NotFoundException>(
                () => repository.GetSecureTokenAsync("non-existent-token", CancellationToken.None)
            );
        }

        [Fact]
        public async Task DeleteSecureTokenAsync_RemovesTokenFromDatabase()
        {
            
            var repository = CreateRepository();
            
            var tokenEntity = new AccountConfirmationTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = "token-to-delete",
                UserId = _testUser.Id,
                User = _testUser,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            };
            
            _context.AccountConfirmationTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            
            await repository.DeleteSecureTokenAsync("token-to-delete", CancellationToken.None);

            
            var deletedToken = await _context.AccountConfirmationTokens
                .FirstOrDefaultAsync(t => t.Token == "token-to-delete");
            Assert.Null(deletedToken);
            
            
            var user = await _context.Users.FindAsync(_testUser.Id);
            Assert.NotNull(user);
        }
}
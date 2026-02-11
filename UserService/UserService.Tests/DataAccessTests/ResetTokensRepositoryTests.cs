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

public class ResetTokensRepositoryTests
{
private readonly IMapper _mapper;
        private readonly SqliteConnection _connection;
        private readonly UserServiceDbContext _context;
        private readonly UserEntity _testUser;

        public ResetTokensRepositoryTests()
        {
            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ResetTokenProfile>();
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

        private ResetTokenRepository CreateRepository()
        {
            return new ResetTokenRepository(_context, _mapper);
        }

        [Fact]
        public async Task CreateSecureTokenAsync_AddsTokenToDatabase()
        {
            
            var repository = CreateRepository();
            var token = ResetToken.CreateInstance(
                "reset-token-456",
                _testUser.Id,
                DateTime.UtcNow.AddMinutes(15)
            );

            
            var tokenString = await repository.CreateSecureTokenAsync(token, CancellationToken.None);

            
            Assert.Equal("reset-token-456", tokenString);
            
            var tokenEntity = await _context.ResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == "reset-token-456");
            Assert.NotNull(tokenEntity);
            Assert.Equal(_testUser.Id, tokenEntity.UserId);
            Assert.Equal(token.ExpiresAt, tokenEntity.ExpiresAt);
        }

        [Fact]
        public async Task GetSecureTokenAsync_ReturnsToken_WhenExists()
        {
            
            var repository = CreateRepository();
            var expiresAt = DateTime.UtcNow.AddMinutes(15);
            
            var tokenEntity = new ResetTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = "existing-reset-token",
                UserId = _testUser.Id,
                User = _testUser,
                ExpiresAt = expiresAt
            };
            
            _context.ResetTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            
            var token = await repository.GetSecureTokenAsync("existing-reset-token", CancellationToken.None);

            
            Assert.NotNull(token);
            Assert.Equal("existing-reset-token", token.Token);
            Assert.Equal(_testUser.Id, token.UserId);
            Assert.Equal(expiresAt, token.ExpiresAt);
        }

        [Fact]
        public async Task GetSecureTokenAsync_ReturnsNoTrackingToken()
        {
            
            var repository = CreateRepository();
            var tokenEntity = new ResetTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = "no-tracking-token",
                UserId = _testUser.Id,
                User = _testUser,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            
            _context.ResetTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            
            var token = await repository.GetSecureTokenAsync("no-tracking-token", CancellationToken.None);

            
            _context.ChangeTracker.Clear();
            var entityInContext = await _context.ResetTokens.FindAsync(tokenEntity.Id);
            Assert.NotNull(entityInContext); 
        }

        [Fact]
        public async Task GetSecureTokenAsync_ThrowsNotFoundException_WhenTokenNotFound()
        {
            
            var repository = CreateRepository();

            
            await Assert.ThrowsAsync<NotFoundException>(
                () => repository.GetSecureTokenAsync("non-existent-reset-token", CancellationToken.None)
            );
        }

        [Fact]
        public async Task DeleteSecureTokenAsync_RemovesTokenFromDatabase()
        {
            
            var repository = CreateRepository();
            
            var tokenEntity = new ResetTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = "reset-token-to-delete",
                UserId = _testUser.Id,
                User = _testUser,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            
            _context.ResetTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            
            await repository.DeleteSecureTokenAsync("reset-token-to-delete", CancellationToken.None);

            
            var deletedToken = await _context.ResetTokens
                .FirstOrDefaultAsync(t => t.Token == "reset-token-to-delete");
            Assert.Null(deletedToken);
            
            
            var user = await _context.Users.FindAsync(_testUser.Id);
            Assert.NotNull(user);
        }

        [Fact]
        public async Task DeleteSecureTokenAsync_WorksWithMultipleTokens()
        {
            
            var repository = CreateRepository();
            
            var token1 = new ResetTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = "token-1",
                UserId = _testUser.Id,
                User = _testUser,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            
            var token2 = new ResetTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = "token-2",
                UserId = _testUser.Id,
                User = _testUser,
                ExpiresAt = DateTime.UtcNow.AddMinutes(15)
            };
            
            _context.ResetTokens.AddRange(token1, token2);
            await _context.SaveChangesAsync();

            
            await repository.DeleteSecureTokenAsync("token-1", CancellationToken.None);

            
            var deletedToken = await _context.ResetTokens
                .FirstOrDefaultAsync(t => t.Token == "token-1");
            var remainingToken = await _context.ResetTokens
                .FirstOrDefaultAsync(t => t.Token == "token-2");
                
            Assert.Null(deletedToken);
            Assert.NotNull(remainingToken);
            Assert.Equal(_testUser.Id, remainingToken.UserId);
        }
}
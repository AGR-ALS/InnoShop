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

public class RefreshTokensRepositoryTests
{
     private readonly IMapper _mapper;
        private readonly SqliteConnection _connection;
        private readonly UserServiceDbContext _context;
        private readonly UserEntity _testUser;

        public RefreshTokensRepositoryTests()
        {
            
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<RefreshTokenProfile>();
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

        private RefreshTokensRepository CreateRepository()
        {
            return new RefreshTokensRepository(_context, _mapper);
        }

        [Fact]
        public async Task CreateSecureTokenAsync_AddsRefreshTokenToDatabase()
        {
            
            var repository = CreateRepository();
            var token = RefreshToken.CreateInstance(
                "refresh-token-789",
                _testUser.Id,
                DateTime.UtcNow.AddDays(7)
            );

            
            var tokenString = await repository.CreateSecureTokenAsync(token, CancellationToken.None);

            
            Assert.Equal("refresh-token-789", tokenString);
            
            var tokenEntity = await _context.RefreshTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == "refresh-token-789");
            Assert.NotNull(tokenEntity);
            Assert.Equal(_testUser.Id, tokenEntity.UserId);
            Assert.Equal(token.ExpiresAt, tokenEntity.ExpiresAt);
        }

        [Fact]
        public async Task GetSecureTokenAsync_ReturnsRefreshToken_WhenExists()
        {
            
            var repository = CreateRepository();
            var expiresAt = DateTime.UtcNow.AddDays(14);
            
            var tokenEntity = new RefreshTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = "existing-refresh-token",
                UserId = _testUser.Id,
                User = _testUser,
                ExpiresAt = expiresAt
            };
            
            _context.RefreshTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            
            var token = await repository.GetSecureTokenAsync("existing-refresh-token", CancellationToken.None);

            
            Assert.NotNull(token);
            Assert.Equal("existing-refresh-token", token.Token);
            Assert.Equal(_testUser.Id, token.UserId);
            Assert.Equal(expiresAt, token.ExpiresAt);
        }

        [Fact]
        public async Task GetSecureTokenAsync_ThrowsNotFoundException_WhenTokenNotFound()
        {
            
            var repository = CreateRepository();

            
            await Assert.ThrowsAsync<NotFoundException>(
                () => repository.GetSecureTokenAsync("non-existent-refresh-token", CancellationToken.None)
            );
        }

        [Fact]
        public async Task DeleteSecureTokenAsync_RemovesRefreshTokenFromDatabase()
        {
            
            var repository = CreateRepository();
            
            var tokenEntity = new RefreshTokenEntity
            {
                Id = Guid.NewGuid(),
                Token = "refresh-token-to-delete",
                UserId = _testUser.Id,
                User = _testUser,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };
            
            _context.RefreshTokens.Add(tokenEntity);
            await _context.SaveChangesAsync();

            
            await repository.DeleteSecureTokenAsync("refresh-token-to-delete", CancellationToken.None);

            
            var deletedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == "refresh-token-to-delete");
            Assert.Null(deletedToken);
            
            
            var user = await _context.Users.FindAsync(_testUser.Id);
            Assert.NotNull(user);
        }

        [Fact]
        public async Task DeleteSecureTokenAsync_WorksWithExecuteDeleteAsync()
        {
            
            var repository = CreateRepository();
            
            
            for (int i = 1; i <= 3; i++)
            {
                var tokenEntity = new RefreshTokenEntity
                {
                    Id = Guid.NewGuid(),
                    Token = $"token-{i}",
                    UserId = _testUser.Id,
                    User = _testUser,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
                
                _context.RefreshTokens.Add(tokenEntity);
            }
            
            await _context.SaveChangesAsync();

            
            await repository.DeleteSecureTokenAsync("token-2", CancellationToken.None);

            
            var remainingTokens = await _context.RefreshTokens
                .Where(t => t.UserId == _testUser.Id)
                .ToListAsync();
                
            Assert.Equal(2, remainingTokens.Count);
            Assert.Contains(remainingTokens, t => t.Token == "token-1");
            Assert.Contains(remainingTokens, t => t.Token == "token-3");
            Assert.DoesNotContain(remainingTokens, t => t.Token == "token-2");
        }

        [Fact]
        public async Task CreateAndRetrieveMultipleTokens_WorksCorrectly()
        {
            
            var repository = CreateRepository();
            
            
            var tokens = new List<RefreshToken>();
            for (int i = 1; i <= 5; i++)
            {
                var token = RefreshToken.CreateInstance(
                    $"multi-token-{i}",
                    _testUser.Id,
                    DateTime.UtcNow.AddDays(i)
                );
                
                await repository.CreateSecureTokenAsync(token, CancellationToken.None);
                tokens.Add(token);
            }

            
            var tokenEntities = await _context.RefreshTokens
                .Where(t => t.UserId == _testUser.Id)
                .ToListAsync();
                
            Assert.Equal(5, tokenEntities.Count);
            
            
            foreach (var expectedToken in tokens)
            {
                var retrievedToken = await repository.GetSecureTokenAsync(expectedToken.Token, CancellationToken.None);
                Assert.Equal(expectedToken.UserId, retrievedToken.UserId);
                Assert.Equal(expectedToken.ExpiresAt, retrievedToken.ExpiresAt);
            }
        }
}
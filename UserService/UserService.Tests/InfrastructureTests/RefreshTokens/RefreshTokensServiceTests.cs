using Microsoft.Extensions.Options;
using Moq;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Repositories;
using UserService.Domain.Models;
using UserService.Infrastructure.Authentication.RefreshTokens;
using Xunit;

namespace UserService.Tests.InfrastructureTests.RefreshTokens;

public class RefreshTokensServiceTests
{
    private readonly Mock<IRefreshTokensRepository> _repositoryMock;
        private readonly Mock<ISecureTokenGenerator> _tokenGeneratorMock;
        private readonly RefreshTokensService _service;
        private readonly RefreshTokenSettings _settings;

        public RefreshTokensServiceTests()
        {
            _repositoryMock = new Mock<IRefreshTokensRepository>();
            _tokenGeneratorMock = new Mock<ISecureTokenGenerator>();
            _settings = new RefreshTokenSettings { ExpiresInDays = 14 };
            
            _service = new RefreshTokensService(
                _tokenGeneratorMock.Object,
                _repositoryMock.Object,
                Options.Create(_settings)
            );
        }

        [Fact]
        public async Task CreateSecureTokenAsync_ReturnsToken_And_ValidateSecureTokenAsync_ReturnsTrue_ForValidToken()
        {
            
            var userId = Guid.NewGuid();
            var tokenString = "generated_token_123";
            var expiresAt = DateTime.UtcNow.AddDays(_settings.ExpiresInDays);
            var refreshToken = RefreshToken.CreateInstance(tokenString, userId, expiresAt);

            _tokenGeneratorMock.Setup(x => x.GenerateToken()).Returns(tokenString);
            _repositoryMock.Setup(x => x.CreateSecureTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenString);
            _repositoryMock.Setup(x => x.GetSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()))
                .ReturnsAsync(refreshToken);
            _tokenGeneratorMock.Setup(x => x.VerifyToken(It.IsAny<RefreshToken>())).Returns(true);

            
            var createdToken = await _service.CreateSecureTokenAsync(userId, CancellationToken.None);
            var isValid = await _service.ValidateSecureTokenAsync(createdToken, CancellationToken.None);

            
            Assert.Equal(tokenString, createdToken);
            Assert.True(isValid);
            
            _repositoryMock.Verify(x => x.CreateSecureTokenAsync(
                It.Is<RefreshToken>(rt => 
                    rt.Token == tokenString && 
                    rt.UserId == userId &&
                    rt.ExpiresAt >= DateTime.UtcNow.AddDays(_settings.ExpiresInDays).AddMinutes(-1) &&
                    rt.ExpiresAt <= DateTime.UtcNow.AddDays(_settings.ExpiresInDays).AddMinutes(1)
                ), 
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task ValidateSecureTokenAsync_ReturnsFalse_ForExpiredToken()
        {
            
            var tokenString = "expired_token";
            var userId = Guid.NewGuid();
            var expiredToken = RefreshToken.CreateInstance(
                tokenString, 
                userId, 
                DateTime.UtcNow.AddDays(-1) 
            );

            _repositoryMock.Setup(x => x.GetSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expiredToken);
            _tokenGeneratorMock.Setup(x => x.VerifyToken(expiredToken)).Returns(false);

            
            var isValid = await _service.ValidateSecureTokenAsync(tokenString, CancellationToken.None);

            
            Assert.False(isValid);
        }
        
        [Fact]
        public async Task DeleteSecureTokenAsync_CallsRepositoryDeleteMethod()
        {
            
            var tokenString = "refresh_token_to_delete";

            
            await _service.DeleteSecureTokenAsync(tokenString, CancellationToken.None);

            
            _repositoryMock.Verify(x => x.DeleteSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetSecureTokenModelAsync_ReturnsRefreshTokenFromRepository()
        {
            
            var tokenString = "test_refresh_token";
            var userId = Guid.NewGuid();
            var expiresAt = DateTime.UtcNow.AddDays(14);
            var expectedToken = RefreshToken.CreateInstance(tokenString, userId, expiresAt);

            _repositoryMock.Setup(x => x.GetSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedToken);

            
            var result = await _service.GetSecureTokenModelAsync(tokenString, CancellationToken.None);

            
            Assert.NotNull(result);
            Assert.Equal(tokenString, result.Token);
            Assert.Equal(userId, result.UserId);
            Assert.Equal(expiresAt, result.ExpiresAt, TimeSpan.FromSeconds(1));
            _repositoryMock.Verify(x => x.GetSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()), Times.Once);
        }
}
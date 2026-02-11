using Microsoft.Extensions.Options;
using Moq;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Repositories;
using UserService.Domain.Models;
using UserService.Infrastructure.Authentication.ResetTokens;
using Xunit;

namespace UserService.Tests.InfrastructureTests.ResetTokens;

public class ResetTokenServiceTests
{
    private readonly Mock<IResetTokenRepository> _repositoryMock;
        private readonly Mock<ISecureTokenGenerator> _tokenGeneratorMock;
        private readonly ResetTokenService _service;
        private readonly ResetTokenSettings _settings;

        public ResetTokenServiceTests()
        {
            _repositoryMock = new Mock<IResetTokenRepository>();
            _tokenGeneratorMock = new Mock<ISecureTokenGenerator>();
            _settings = new ResetTokenSettings { ExpiresInMinutes = 5 };
            
            _service = new ResetTokenService(
                Options.Create(_settings),
                _repositoryMock.Object,
                _tokenGeneratorMock.Object
            );
        }

        [Fact]
        public async Task CreateSecureTokenAsync_ReturnsToken_And_ValidateSecureTokenAsync_ReturnsTrue_ForValidToken()
        {
            
            var userId = Guid.NewGuid();
            var tokenString = "reset_token_456";
            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes);
            var resetToken = ResetToken.CreateInstance(tokenString, userId, expiresAt);

            _tokenGeneratorMock.Setup(x => x.GenerateToken()).Returns(tokenString);
            _repositoryMock.Setup(x => x.CreateSecureTokenAsync(It.IsAny<ResetToken>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenString);
            _repositoryMock.Setup(x => x.GetSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resetToken);
            _tokenGeneratorMock.Setup(x => x.VerifyToken(It.IsAny<ResetToken>())).Returns(true);

            
            var createdToken = await _service.CreateSecureTokenAsync(userId, CancellationToken.None);
            var isValid = await _service.ValidateSecureTokenAsync(createdToken, CancellationToken.None);

            
            Assert.Equal(tokenString, createdToken);
            Assert.True(isValid);
            
            _repositoryMock.Verify(x => x.CreateSecureTokenAsync(
                It.Is<ResetToken>(rt => 
                    rt.Token == tokenString && 
                    rt.UserId == userId &&
                    rt.ExpiresAt >= DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes).AddSeconds(-5) &&
                    rt.ExpiresAt <= DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes).AddSeconds(5)
                ), 
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task GetSecureTokenModelAsync_ReturnsCorrectToken()
        {
            
            var tokenString = "test_token";
            var userId = Guid.NewGuid();
            var resetToken = ResetToken.CreateInstance(tokenString, userId, DateTime.UtcNow.AddMinutes(5));

            _repositoryMock.Setup(x => x.GetSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()))
                .ReturnsAsync(resetToken);

            
            var result = await _service.GetSecureTokenModelAsync(tokenString, CancellationToken.None);

            
            Assert.NotNull(result);
            Assert.Equal(tokenString, result.Token);
            Assert.Equal(userId, result.UserId);
            _repositoryMock.Verify(x => x.GetSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task DeleteSecureTokenAsync_CallsRepositoryDeleteMethod()
        {
            
            var tokenString = "test_token_to_delete";

            
            await _service.DeleteSecureTokenAsync(tokenString, CancellationToken.None);

            
            _repositoryMock.Verify(x => x.DeleteSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()), Times.Once);
        }
}
using Microsoft.Extensions.Options;
using Moq;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Repositories;
using UserService.Domain.Models;
using UserService.Infrastructure.Authentication.AccountConfirmationTokens;
using Xunit;

namespace UserService.Tests.InfrastructureTests.AccountConfirmationTokens;

public class AccountConfirmationTokenServiceTests
{
    private readonly Mock<IAccountConfirmationTokenRepository> _repositoryMock;
        private readonly Mock<ISecureTokenGenerator> _tokenGeneratorMock;
        private readonly AccountConfirmationTokenService _service;
        private readonly AccountConfirmationTokenSettings _settings;

        public AccountConfirmationTokenServiceTests()
        {
            _repositoryMock = new Mock<IAccountConfirmationTokenRepository>();
            _tokenGeneratorMock = new Mock<ISecureTokenGenerator>();
            _settings = new AccountConfirmationTokenSettings { ExpiresInMinutes = 5 };
            
            _service = new AccountConfirmationTokenService(
                Options.Create(_settings),
                _repositoryMock.Object,
                _tokenGeneratorMock.Object
            );
        }

        [Fact]
        public async Task CreateSecureTokenAsync_ReturnsToken_And_ValidateSecureTokenAsync_ReturnsTrue_ForValidToken()
        {
            
            var userId = Guid.NewGuid();
            var tokenString = "confirmation_token_789";
            var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes);
            var confirmationToken = AccountConfirmationToken.CreateInstance(tokenString, userId, expiresAt);

            _tokenGeneratorMock.Setup(x => x.GenerateToken()).Returns(tokenString);
            _repositoryMock.Setup(x => x.CreateSecureTokenAsync(It.IsAny<AccountConfirmationToken>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(tokenString);
            _repositoryMock.Setup(x => x.GetSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()))
                .ReturnsAsync(confirmationToken);
            _tokenGeneratorMock.Setup(x => x.VerifyToken(It.IsAny<AccountConfirmationToken>())).Returns(true);

            
            var createdToken = await _service.CreateSecureTokenAsync(userId, CancellationToken.None);
            var isValid = await _service.ValidateSecureTokenAsync(createdToken, CancellationToken.None);

            
            Assert.Equal(tokenString, createdToken);
            Assert.True(isValid);
            
            _repositoryMock.Verify(x => x.CreateSecureTokenAsync(
                It.Is<AccountConfirmationToken>(act => 
                    act.Token == tokenString && 
                    act.UserId == userId &&
                    act.ExpiresAt >= DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes).AddSeconds(-5) &&
                    act.ExpiresAt <= DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes).AddSeconds(5)
                ), 
                It.IsAny<CancellationToken>()
            ), Times.Once);
        }

        [Fact]
        public async Task DeleteSecureTokenAsync_CallsRepositoryDelete()
        {
            
            var tokenString = "token_to_delete";

            
            await _service.DeleteSecureTokenAsync(tokenString, CancellationToken.None);

            
            _repositoryMock.Verify(x => x.DeleteSecureTokenAsync(tokenString, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task GetSecureTokenModelAsync_ReturnsTokenFromRepository()
        {
            
            var tokenString = "test_confirmation_token";
            var userId = Guid.NewGuid();
            var expiresAt = DateTime.UtcNow.AddMinutes(5);
            var expectedToken = AccountConfirmationToken.CreateInstance(tokenString, userId, expiresAt);

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
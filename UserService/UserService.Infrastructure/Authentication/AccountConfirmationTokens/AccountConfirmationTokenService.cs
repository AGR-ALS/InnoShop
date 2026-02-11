using Microsoft.Extensions.Options;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Domain.Models;

namespace UserService.Infrastructure.Authentication.AccountConfirmationTokens;

public class AccountConfirmationTokenService : IAccountConfirmationTokenService
{
    private readonly IAccountConfirmationTokenRepository _accountConfirmationTokenRepository;
    private readonly ISecureTokenGenerator _secureTokenGenerator;
    private readonly AccountConfirmationTokenSettings _accountConfirmationTokenSettings;

    public AccountConfirmationTokenService(IOptions<AccountConfirmationTokenSettings> accountConfirmationTokenSettings, IAccountConfirmationTokenRepository accountConfirmationTokenRepository, ISecureTokenGenerator secureTokenGenerator)
    {
        _accountConfirmationTokenRepository = accountConfirmationTokenRepository;
        _secureTokenGenerator = secureTokenGenerator;
        _accountConfirmationTokenSettings = accountConfirmationTokenSettings.Value;
    }
    public async Task<string> CreateSecureTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var token = AccountConfirmationToken.CreateInstance(
            _secureTokenGenerator.GenerateToken(),
            userId,
            DateTime.UtcNow.AddMinutes(_accountConfirmationTokenSettings.ExpiresInMinutes)
        );
        var tokenString = await _accountConfirmationTokenRepository.CreateSecureTokenAsync(token, cancellationToken);
        return tokenString;
    }

    public async Task DeleteSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        await _accountConfirmationTokenRepository.DeleteSecureTokenAsync(token, cancellationToken);
    }

    public async Task<bool> ValidateSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        var tokenModel = await _accountConfirmationTokenRepository.GetSecureTokenAsync(token, cancellationToken);
        return _secureTokenGenerator.VerifyToken(tokenModel);
    }

    public async Task<AccountConfirmationToken> GetSecureTokenModelAsync(string token, CancellationToken cancellationToken)
    {
        return await _accountConfirmationTokenRepository.GetSecureTokenAsync(token, cancellationToken);
    }
}
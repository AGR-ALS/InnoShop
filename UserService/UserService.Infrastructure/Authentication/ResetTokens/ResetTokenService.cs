using Microsoft.Extensions.Options;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Domain.Models;
using UserService.Infrastructure.Authentication.RefreshTokens;

namespace UserService.Infrastructure.Authentication.ResetTokens;

public class ResetTokenService : IResetTokenService
{
    private readonly IResetTokenRepository _resetTokenRepository;
    private readonly ISecureTokenGenerator _secureTokenGenerator;
    private readonly ResetTokenSettings _resetTokenSettings;

    public ResetTokenService(IOptions<ResetTokenSettings> refreshTokenSettings, IResetTokenRepository resetTokenRepository, ISecureTokenGenerator secureTokenGenerator)
    {
        _resetTokenRepository = resetTokenRepository;
        _secureTokenGenerator = secureTokenGenerator;
        _resetTokenSettings = refreshTokenSettings.Value;
    }
    public async Task<string> CreateSecureTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var token = ResetToken.CreateInstance(
            _secureTokenGenerator.GenerateToken(),
            userId,
            DateTime.UtcNow.AddMinutes(_resetTokenSettings.ExpiresInMinutes)
            );
        var tokenString = await _resetTokenRepository.CreateSecureTokenAsync(token, cancellationToken);
        return tokenString;
    }

    public async Task DeleteSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        await _resetTokenRepository.DeleteSecureTokenAsync(token, cancellationToken);
    }

    public async Task<bool> ValidateSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        var tokenModel = await _resetTokenRepository.GetSecureTokenAsync(token, cancellationToken);
        return _secureTokenGenerator.VerifyToken(tokenModel);
    }

    public async Task<ResetToken> GetSecureTokenModelAsync(string token, CancellationToken cancellationToken)
    {
        return await _resetTokenRepository.GetSecureTokenAsync(token, cancellationToken);
    }
}
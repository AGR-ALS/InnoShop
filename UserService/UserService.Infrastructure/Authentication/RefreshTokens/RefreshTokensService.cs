using Microsoft.Extensions.Options;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Domain.Models;

namespace UserService.Infrastructure.Authentication.RefreshTokens;

public class RefreshTokensService : IRefreshTokensService
{
    private readonly ISecureTokenGenerator _secureTokenGenerator;
    private readonly IRefreshTokensRepository _refreshTokensRepository;
    private readonly RefreshTokenSettings _refreshTokenSettings;

    public RefreshTokensService(ISecureTokenGenerator secureTokenGenerator, IRefreshTokensRepository refreshTokensRepository, IOptions<RefreshTokenSettings> refreshTokenSettings)
    {
        _secureTokenGenerator = secureTokenGenerator;
        _refreshTokensRepository = refreshTokensRepository;
        _refreshTokenSettings = refreshTokenSettings.Value;
    }

    public async Task<string> CreateSecureTokenAsync(Guid userId, CancellationToken cancellationToken)
    {
        var token = RefreshToken.CreateInstance(
            _secureTokenGenerator.GenerateToken(),
            userId,
            DateTime.UtcNow.AddDays(_refreshTokenSettings.ExpiresInDays)
        );
        return await _refreshTokensRepository.CreateSecureTokenAsync(token, cancellationToken);
    }

    public async Task DeleteSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        await _refreshTokensRepository.DeleteSecureTokenAsync(token, cancellationToken);
    }

    public async Task<bool> ValidateSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        var refreshToken = await _refreshTokensRepository.GetSecureTokenAsync(token, cancellationToken);
        
        return _secureTokenGenerator.VerifyToken(refreshToken);
    }

    public async Task<RefreshToken> GetSecureTokenModelAsync(string token, CancellationToken cancellationToken)
    {
        return await _refreshTokensRepository.GetSecureTokenAsync(token, cancellationToken);
    }
}
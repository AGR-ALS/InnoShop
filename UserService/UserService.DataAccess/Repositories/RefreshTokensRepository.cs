using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Exceptions;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess.Repositories;

public class RefreshTokensRepository : IRefreshTokensRepository
{
    private readonly UserServiceDbContext _dbContext;
    private readonly IMapper _mapper;

    public RefreshTokensRepository(UserServiceDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<string> CreateSecureTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        var refreshTokenEntity = _mapper.Map<RefreshTokenEntity>(refreshToken);
        await _dbContext.AddAsync(refreshTokenEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return refreshTokenEntity.Token;
    }

    public async Task DeleteSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        await _dbContext.RefreshTokens.Where(r=>r.Token == token).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<RefreshToken> GetSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        var refreshTokenEntity = await _dbContext.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
        if(refreshTokenEntity == null)
            throw new NotFoundException("Refresh token was not found");
        return _mapper.Map<RefreshToken>(refreshTokenEntity);
    }
}
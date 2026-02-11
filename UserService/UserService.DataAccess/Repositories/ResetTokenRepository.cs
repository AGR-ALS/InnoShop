using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Exceptions;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess.Repositories;

public class ResetTokenRepository : IResetTokenRepository
{
    private readonly UserServiceDbContext _dbContext;
    private readonly IMapper _mapper;

    public ResetTokenRepository(UserServiceDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<ResetToken> GetSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        var tokenEntity = await _dbContext.ResetTokens.AsNoTracking().FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
        if (tokenEntity == null)
            throw new NotFoundException("Reset token was not found");
        return _mapper.Map<ResetToken>(tokenEntity);
    }

    public async Task<string> CreateSecureTokenAsync(ResetToken resetToken, CancellationToken cancellationToken)
    {
        var tokenEntity = _mapper.Map<ResetTokenEntity>(resetToken);
        await _dbContext.ResetTokens.AddAsync(tokenEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return tokenEntity.Token;
    }

    public async Task DeleteSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        await _dbContext.ResetTokens.Where(x => x.Token == token).ExecuteDeleteAsync(cancellationToken);
    }
}
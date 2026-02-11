using AutoMapper;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Exceptions;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess.Repositories;

public class AccountConfirmationTokenRepository : IAccountConfirmationTokenRepository
{
    private readonly UserServiceDbContext _dbContext;
    private readonly IMapper _mapper;

    public AccountConfirmationTokenRepository(UserServiceDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<string> CreateSecureTokenAsync(AccountConfirmationToken refreshToken, CancellationToken cancellationToken)
    {
        var accountConfirmationTokenEntity = _mapper.Map<AccountConfirmationTokenEntity>(refreshToken);
        await _dbContext.AddAsync(accountConfirmationTokenEntity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return accountConfirmationTokenEntity.Token;
    }

    public async Task DeleteSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        await _dbContext.AccountConfirmationTokens.Where(r=>r.Token == token).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<AccountConfirmationToken> GetSecureTokenAsync(string token, CancellationToken cancellationToken)
    {
        var accountConfirmationTokenEntity = await _dbContext.AccountConfirmationTokens.FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
        if(accountConfirmationTokenEntity == null)
            throw new NotFoundException("Account confirmation token was not found");
        return _mapper.Map<AccountConfirmationToken>(accountConfirmationTokenEntity);
    }
}
// using AutoMapper;
// using Microsoft.EntityFrameworkCore;
// using UserService.Application.Abstractions.Repositories;
// using UserService.Application.Exceptions;
// using UserService.Domain.Models;
//
// namespace UserService.DataAccess.Repositories;
//
// public class SecureTokenRepository<T> : ISecureTokenRepository<T> where T: SecureToken
// {
//     protected readonly UserServiceDbContext _dbContext;
//     protected readonly IMapper _mapper;
//     
//     public SecureTokenRepository(UserServiceDbContext dbContext, IMapper mapper)
//     {
//         _dbContext = dbContext;
//         _mapper = mapper;
//     }
//
//     public async Task<T> GetSecureTokenAsync(string token, CancellationToken cancellationToken)
//     {
//         var tokenEntity = await _dbContext.SecureTokens.AsNoTracking().FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
//         if (tokenEntity == null)
//             throw new NotFoundException("Reset token was not found");
//         return _mapper.Map<T>(tokenEntity);
//     }
//
//     public async Task<string> CreateSecureTokenAsync(T secureToken, CancellationToken cancellationToken)
//     {
//         var secureTokenEntity = _mapper.Map<T>(secureToken);
//         await _dbContext.AddAsync(secureTokenEntity, cancellationToken);
//         await _dbContext.SaveChangesAsync(cancellationToken);
//         return secureTokenEntity.Token;
//     }
//
//     public async Task DeleteSecureTokenAsync(string token, CancellationToken cancellationToken)
//     {
//         await _dbContext.SecureTokens.Where(r=>r.Token == token).ExecuteDeleteAsync(cancellationToken);
//     }
// }
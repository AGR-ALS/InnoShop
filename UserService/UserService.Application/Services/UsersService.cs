using System.Security.Authentication;
using Microsoft.Extensions.Options;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Authentication.Jwt;
using UserService.Application.Abstractions.MessageEvents;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Application.Exceptions;
using UserService.Application.Models;
using UserService.Application.Settings.Roles;
using UserService.Domain.Models;

namespace UserService.Application.Services;

public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokensService _refreshTokensService;
    private readonly IResetTokenService _resetTokenService;
    private readonly IAccountConfirmationTokenService _accountConfirmationTokenService;
    private readonly IMailEventPublisher _mailEventPublisher;
    private readonly IRolesService _rolesService;
    private readonly EmailContents _emailContents;
    private readonly DefaultRoleSettings _defaultRoleSettings;

    public UsersService(IUsersRepository usersRepository, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, 
        IRefreshTokensService refreshTokensService, IOptions<DefaultRoleSettings> defaultRoleSettings, IResetTokenService resetTokenService,
        IOptions<EmailContents> emailContents, IAccountConfirmationTokenService accountConfirmationTokenService,
        IMailEventPublisher mailEventPublisher, IRolesService rolesService)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokensService = refreshTokensService;
        _resetTokenService = resetTokenService;
        _accountConfirmationTokenService = accountConfirmationTokenService;
        _mailEventPublisher = mailEventPublisher;
        _rolesService = rolesService;
        _emailContents = emailContents.Value;
        _defaultRoleSettings = defaultRoleSettings.Value;
    }
    public async Task RegisterUserAsync(string username, string email, string password, CancellationToken cancellationToken)
    {
        var hashedPassword = _passwordHasher.HashPassword(password);
        var user = User.CreateInstance(username, email, hashedPassword);
        user.RoleId = (await _rolesService.GetRoleByNameAsync(_defaultRoleSettings.Role, cancellationToken)).Id;
        try
        {
            await _usersRepository.CreateUserAsync(user, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DbCreatingException(ex.InnerException?.Message!);
        }
    }

    public async Task<(string, string)> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetUserByEmailAsync(email, cancellationToken);
        var loginResult = _passwordHasher.VerifyHashedPassword(password, user.PasswordHash);
        if (!loginResult)
        {
            throw new InvalidCredentialException("Invalid login or password");
        }
        var accessToken = _jwtTokenGenerator.GenerateJwtToken(user!);
        var refreshToken = await _refreshTokensService.CreateSecureTokenAsync(user.Id, cancellationToken);
        return (accessToken, refreshToken);
    }

    public async Task<string> LoginAsync(string passedRefreshToken, CancellationToken cancellationToken)
    {
        if (!(await _refreshTokensService.ValidateSecureTokenAsync(passedRefreshToken, cancellationToken)))
        {
            await _refreshTokensService.DeleteSecureTokenAsync(passedRefreshToken, cancellationToken);
            throw new UnauthorizedAccessException("Invalid refresh token");
        }
        var refreshToken = await _refreshTokensService.GetSecureTokenModelAsync(passedRefreshToken, cancellationToken);
        var user = await _usersRepository.GetUserByIdAsync(refreshToken.UserId, cancellationToken);
        var accessToken = _jwtTokenGenerator.GenerateJwtToken(user!);
        return accessToken;
    }

    public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return await _usersRepository.GetAllUsersAsync(cancellationToken);
    }

    public async Task<User> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _usersRepository.GetUserByIdAsync(id, cancellationToken);
    }

    public async Task<User> GetUserByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _usersRepository.GetUserByEmailAsync(email, cancellationToken);
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        await _usersRepository.UpdateUserAsync(user, cancellationToken);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken cancellationToken)
    {
        await _usersRepository.DeleteUserAsync(id, cancellationToken);
    }

    public async Task SendResetPasswordRequestAsync(string email, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetUserByEmailAsync(email, cancellationToken);
        if(user == null)
            throw new NotFoundException("Email was not found");
        var tokenString = await _resetTokenService.CreateSecureTokenAsync(user.Id, cancellationToken);
        await _mailEventPublisher.SendMail(user.Email, _emailContents.ForRestoringPassword.Subject, _emailContents.BuildMessage(_emailContents.ForRestoringPassword.Body, _emailContents.BuildLink(_emailContents.ForRestoringPassword, tokenString)));
    }

    public async Task SetNewPasswordAsync(string token, string password,
        CancellationToken cancellationToken)
    {
        var isTokenValid = await  _resetTokenService.ValidateSecureTokenAsync(token, cancellationToken);
        if (isTokenValid == false)
        {
            throw new UnauthorizedAccessException("Reset token has expired");
        }
        var tokenModel = await _resetTokenService.GetSecureTokenModelAsync(token, cancellationToken);
        await _usersRepository.ResetPasswordAsync(tokenModel.UserId, _passwordHasher.HashPassword(password), cancellationToken);
        await _resetTokenService.DeleteSecureTokenAsync(token, cancellationToken);
    }
    
    public async Task SendAccountConfirmationEmail(string email, CancellationToken cancellationToken)
    {
        var user = await _usersRepository.GetUserByEmailAsync(email, cancellationToken);
        if(user == null)
            throw new NotFoundException("Email was not found");
        var tokenString = await _accountConfirmationTokenService.CreateSecureTokenAsync(user.Id, cancellationToken);
        await _mailEventPublisher.SendMail(user.Email, _emailContents.ForAccountConfirmation.Subject, _emailContents.BuildMessage(_emailContents.ForRestoringPassword.Body, _emailContents.BuildLink(_emailContents.ForAccountConfirmation, tokenString))); 
    }
    
    public async Task SetAccountAsConfirmed(string token, CancellationToken cancellationToken)
    {
        var isTokenValid = await  _accountConfirmationTokenService.ValidateSecureTokenAsync(token, cancellationToken);
        if (isTokenValid == false)
        {
            throw new UnauthorizedAccessException("Account confirmation token has expired");
        }
        var tokenModel = await _accountConfirmationTokenService.GetSecureTokenModelAsync(token, cancellationToken);
        await _usersRepository.SetUserConfirmedAsync(tokenModel.UserId, true, cancellationToken);
        await _accountConfirmationTokenService.DeleteSecureTokenAsync(token, cancellationToken);
    }

    public async Task SetAccountActiveAsync(Guid id, bool isActive, CancellationToken cancellationToken)
    {
        await _usersRepository.SetUserActiveAsync(id, isActive, cancellationToken);
    }
}
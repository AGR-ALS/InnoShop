using System.Security.Authentication;
using Microsoft.Extensions.Options;
using Moq;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Authentication.Jwt;
using UserService.Application.Abstractions.MessageEvents;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Application.Exceptions;
using UserService.Application.Models;
using UserService.Application.Services;
using UserService.Application.Settings.Roles;
using UserService.Domain.Models;
using Xunit;

namespace UserService.Tests.ApplicationTests;

public class UsersServiceTests
{
    private readonly Mock<IUsersRepository> _usersRepositoryMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IRefreshTokensService> _refreshTokensServiceMock;
    private readonly Mock<IResetTokenService> _resetTokenServiceMock;
    private readonly Mock<IAccountConfirmationTokenService> _accountConfirmationTokenServiceMock;
    private readonly Mock<IMailEventPublisher> _mailEventPublisherMock;
    private readonly Mock<IRolesService> _rolesServiceMock;
    private readonly EmailContents _emailContents;
    private readonly DefaultRoleSettings _defaultRoleSettings;
    private readonly UsersService _service;

    public UsersServiceTests()
    {
        _usersRepositoryMock = new Mock<IUsersRepository>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _refreshTokensServiceMock = new Mock<IRefreshTokensService>();
        _resetTokenServiceMock = new Mock<IResetTokenService>();
        _accountConfirmationTokenServiceMock = new Mock<IAccountConfirmationTokenService>();
        _mailEventPublisherMock = new Mock<IMailEventPublisher>();
        _rolesServiceMock = new Mock<IRolesService>();

        _emailContents = new EmailContents
        {
            ForRestoringPassword = new EmailContent { Subject = "Reset", Body = "Reset body" },
            ForAccountConfirmation = new EmailContent { Subject = "Confirm", Body = "Confirm body" }
        };

        _defaultRoleSettings = new DefaultRoleSettings { Role = "Regular" };

        _service = new UsersService(
            _usersRepositoryMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _refreshTokensServiceMock.Object,
            Options.Create(_defaultRoleSettings),
            _resetTokenServiceMock.Object,
            Options.Create(_emailContents),
            _accountConfirmationTokenServiceMock.Object,
            _mailEventPublisherMock.Object,
            _rolesServiceMock.Object
        );
    }

    [Fact]
    public async Task RegisterUserAsync_CallsCreateUserAsync_WithHashedPasswordAndDefaultRole()
    {
        var username = "testuser";
        var email = "test@example.com";
        var password = "password123";
        var hashedPassword = "hashed123";
        var roleId = Guid.NewGuid();
        var role = Role.CreateInstance("Regular");
        role.Id = roleId;

        _passwordHasherMock
            .Setup(h => h.HashPassword(password))
            .Returns(hashedPassword);

        _rolesServiceMock
            .Setup(r => r.GetRoleByNameAsync(_defaultRoleSettings.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        await _service.RegisterUserAsync(username, email, password, CancellationToken.None);

        _usersRepositoryMock.Verify(r => r.CreateUserAsync(
            It.Is<User>(u =>
                u.Name == username &&
                u.Email == email &&
                u.PasswordHash == hashedPassword &&
                u.RoleId == roleId),
            CancellationToken.None
        ), Times.Once);
    }

    [Fact]
    public async Task RegisterUserAsync_OnException_ThrowsDbCreatingException()
    {
        var role = Role.CreateInstance("Regular");
        role.Id = Guid.NewGuid();
        var innerException = new Exception("Database error");
        
        _rolesServiceMock
            .Setup(r => r.GetRoleByNameAsync(_defaultRoleSettings.Role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _usersRepositoryMock
            .Setup(r => r.CreateUserAsync(It.IsAny<User>(), CancellationToken.None))
            .ThrowsAsync(new Exception("Error", innerException));

        await Assert.ThrowsAsync<DbCreatingException>(
            () => _service.RegisterUserAsync("u", "e", "p", CancellationToken.None)
        );
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsAccessAndRefreshTokens()
    {
        var email = "test@example.com";
        var password = "password123";
        var role = Role.CreateInstance("Regular");
        role.Id = Guid.NewGuid();
        var user = User.CreateInstance("testuser", email, "hashedPassword");
        user.Role = role;
        var accessToken = "access-token";
        var refreshToken = "refresh-token";

        _usersRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, CancellationToken.None))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(password, user.PasswordHash))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateJwtToken(user))
            .Returns(accessToken);

        _refreshTokensServiceMock
            .Setup(r => r.CreateSecureTokenAsync(user.Id, CancellationToken.None))
            .ReturnsAsync(refreshToken);

        var (resultAccessToken, resultRefreshToken) =
            await _service.LoginAsync(email, password, CancellationToken.None);

        Assert.Equal(accessToken, resultAccessToken);
        Assert.Equal(refreshToken, resultRefreshToken);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ThrowsInvalidCredentialException()
    {
        var email = "test@example.com";
        var role = Role.CreateInstance("Regular");
        role.Id = Guid.NewGuid();
        var user = User.CreateInstance("testuser", email, "hashedPassword");
        user.Role = role;

        _usersRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, CancellationToken.None))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.VerifyHashedPassword(It.IsAny<string>(), user.PasswordHash))
            .Returns(false);

        await Assert.ThrowsAsync<InvalidCredentialException>(
            () => _service.LoginAsync(email, "wrongpass", CancellationToken.None)
        );
    }

    [Fact]
    public async Task LoginAsync_WithRefreshToken_ReturnsNewAccessToken()
    {
        var refreshToken = "valid-refresh-token";
        var userId = Guid.NewGuid();
        var role = Role.CreateInstance("Regular");
        role.Id = Guid.NewGuid();
        var user = User.CreateInstance("testuser", "test@example.com", "hashedPassword");
        user.Role = role;
        var accessToken = "new-access-token";
        var tokenModel = RefreshToken.CreateInstance(refreshToken, userId, DateTime.UtcNow.AddDays(1));

        _refreshTokensServiceMock
            .Setup(r => r.ValidateSecureTokenAsync(refreshToken, CancellationToken.None))
            .ReturnsAsync(true);

        _refreshTokensServiceMock
            .Setup(r => r.GetSecureTokenModelAsync(refreshToken, CancellationToken.None))
            .ReturnsAsync(tokenModel);

        _usersRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, CancellationToken.None))
            .ReturnsAsync(user);

        _jwtTokenGeneratorMock
            .Setup(j => j.GenerateJwtToken(user))
            .Returns(accessToken);

        var result = await _service.LoginAsync(refreshToken, CancellationToken.None);

        Assert.Equal(accessToken, result);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidRefreshToken_ThrowsUnauthorizedAccessException()
    {
        var refreshToken = "invalid-refresh-token";

        _refreshTokensServiceMock
            .Setup(r => r.ValidateSecureTokenAsync(refreshToken, CancellationToken.None))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.LoginAsync(refreshToken, CancellationToken.None)
        );

        _refreshTokensServiceMock.Verify(r =>
            r.DeleteSecureTokenAsync(refreshToken, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetUsersAsync_CallsGetAllUsersAsync()
    {
        var role = Role.CreateInstance("Regular");
        role.Id = Guid.NewGuid();
        var user = User.CreateInstance("user1", "email1", "hash");
        user.Role = role;
        var users = new List<User> { user };

        _usersRepositoryMock
            .Setup(r => r.GetAllUsersAsync(CancellationToken.None))
            .ReturnsAsync(users);

        var result = await _service.GetUsersAsync(CancellationToken.None);

        Assert.Equal(users, result);
    }

    [Fact]
    public async Task GetUserByIdAsync_CallsGetUserByIdAsync()
    {
        var userId = Guid.NewGuid();
        var role = Role.CreateInstance("Regular");
        role.Id = Guid.NewGuid();
        var user = User.CreateInstance("test", "test@example.com", "hash");
        user.Role = role;

        _usersRepositoryMock
            .Setup(r => r.GetUserByIdAsync(userId, CancellationToken.None))
            .ReturnsAsync(user);

        var result = await _service.GetUserByIdAsync(userId, CancellationToken.None);

        Assert.Equal(user, result);
    }

    [Fact]
    public async Task GetUserByEmailAsync_CallsGetUserByEmailAsync()
    {
        var email = "test@example.com";
        var role = Role.CreateInstance("Regular");
        role.Id = Guid.NewGuid();
        var user = User.CreateInstance("test", email, "hash");
        user.Role = role;

        _usersRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, CancellationToken.None))
            .ReturnsAsync(user);

        var result = await _service.GetUserByEmailAsync(email, CancellationToken.None);

        Assert.Equal(user, result);
    }

    [Fact]
    public async Task UpdateUserAsync_CallsUpdateUserAsync()
    {
        var role = Role.CreateInstance("Admin");
        role.Id = Guid.NewGuid();
        var user = new User { Id = Guid.NewGuid(), Name = "newName", Email = "new@example.com", Role = role };

        await _service.UpdateUserAsync(user, CancellationToken.None);

        _usersRepositoryMock.Verify(r =>
            r.UpdateUserAsync(user, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task DeleteUserAsync_CallsDeleteUserAsync()
    {
        var userId = Guid.NewGuid();

        await _service.DeleteUserAsync(userId, CancellationToken.None);

        _usersRepositoryMock.Verify(r =>
            r.DeleteUserAsync(userId, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SendResetPasswordRequestAsync_WithValidEmail_SendsEmail()
    {
        var email = "test@example.com";
        var userId = Guid.NewGuid();
        var role = Role.CreateInstance("Regular");
        role.Id = Guid.NewGuid();
        var user = User.CreateInstance("test", email, "hash");
        user.Id = userId;
        user.Role = role;
        var token = "reset-token";

        _usersRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, CancellationToken.None))
            .ReturnsAsync(user);

        _resetTokenServiceMock
            .Setup(r => r.CreateSecureTokenAsync(userId, CancellationToken.None))
            .ReturnsAsync(token);

        await _service.SendResetPasswordRequestAsync(email, CancellationToken.None);

        _mailEventPublisherMock.Verify(m =>
            m.SendMail(email, _emailContents.ForRestoringPassword.Subject,
                It.Is<string>(s => s.Contains(token))), Times.Once);
    }

    [Fact]
    public async Task SendResetPasswordRequestAsync_WithInvalidEmail_ThrowsNotFoundException()
    {
        var email = "nonexistent@example.com";

        _usersRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, CancellationToken.None))
            .ReturnsAsync((User)null);

        await Assert.ThrowsAsync<NotFoundException>(
            () => _service.SendResetPasswordRequestAsync(email, CancellationToken.None)
        );
    }

    [Fact]
    public async Task SetNewPasswordAsync_WithValidToken_ResetsPassword()
    {
        var token = "valid-token";
        var password = "newPassword123";
        var userId = Guid.NewGuid();
        var hashedPassword = "hashedNewPassword";
        var tokenModel = ResetToken.CreateInstance(token, userId, DateTime.UtcNow.AddMinutes(10));

        _resetTokenServiceMock
            .Setup(r => r.ValidateSecureTokenAsync(token, CancellationToken.None))
            .ReturnsAsync(true);

        _resetTokenServiceMock
            .Setup(r => r.GetSecureTokenModelAsync(token, CancellationToken.None))
            .ReturnsAsync(tokenModel);

        _passwordHasherMock
            .Setup(h => h.HashPassword(password))
            .Returns(hashedPassword);

        await _service.SetNewPasswordAsync(token, password, CancellationToken.None);

        _usersRepositoryMock.Verify(r =>
            r.ResetPasswordAsync(userId, hashedPassword, CancellationToken.None), Times.Once);
        _resetTokenServiceMock.Verify(r =>
            r.DeleteSecureTokenAsync(token, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SetNewPasswordAsync_WithInvalidToken_ThrowsUnauthorizedAccessException()
    {
        var token = "invalid-token";

        _resetTokenServiceMock
            .Setup(r => r.ValidateSecureTokenAsync(token, CancellationToken.None))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.SetNewPasswordAsync(token, "newpass", CancellationToken.None)
        );
    }

    [Fact]
    public async Task SendAccountConfirmationEmail_WithValidEmail_SendsEmail()
    {
        var email = "test@example.com";
        var userId = Guid.NewGuid();
        var role = Role.CreateInstance("Regular");
        role.Id = Guid.NewGuid();
        var user = User.CreateInstance("test", email, "hash");
        user.Id = userId;
        user.Role = role;
        var token = "confirmation-token";

        _usersRepositoryMock
            .Setup(r => r.GetUserByEmailAsync(email, CancellationToken.None))
            .ReturnsAsync(user);

        _accountConfirmationTokenServiceMock
            .Setup(a => a.CreateSecureTokenAsync(userId, CancellationToken.None))
            .ReturnsAsync(token);

        await _service.SendAccountConfirmationEmail(email, CancellationToken.None);

        _mailEventPublisherMock.Verify(m =>
            m.SendMail(email, _emailContents.ForAccountConfirmation.Subject,
                It.Is<string>(s => s.Contains(token))), Times.Once);
    }

    [Fact]
    public async Task SetAccountAsConfirmed_WithValidToken_ConfirmsAccount()
    {
        var token = "valid-token";
        var userId = Guid.NewGuid();
        var tokenModel = AccountConfirmationToken.CreateInstance(token, userId, DateTime.UtcNow.AddMinutes(10));

        _accountConfirmationTokenServiceMock
            .Setup(a => a.ValidateSecureTokenAsync(token, CancellationToken.None))
            .ReturnsAsync(true);

        _accountConfirmationTokenServiceMock
            .Setup(a => a.GetSecureTokenModelAsync(token, CancellationToken.None))
            .ReturnsAsync(tokenModel);

        await _service.SetAccountAsConfirmed(token, CancellationToken.None);

        _usersRepositoryMock.Verify(r =>
            r.SetUserConfirmedAsync(userId, true, CancellationToken.None), Times.Once);
        _accountConfirmationTokenServiceMock.Verify(a =>
            a.DeleteSecureTokenAsync(token, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task SetAccountAsConfirmed_WithInvalidToken_ThrowsUnauthorizedAccessException()
    {
        var token = "invalid-token";

        _accountConfirmationTokenServiceMock
            .Setup(a => a.ValidateSecureTokenAsync(token, CancellationToken.None))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _service.SetAccountAsConfirmed(token, CancellationToken.None)
        );
    }

    [Fact]
    public async Task SetAccountActiveAsync_CallsSetUserActiveAsync()
    {
        var userId = Guid.NewGuid();
        var isActive = true;

        await _service.SetAccountActiveAsync(userId, isActive, CancellationToken.None);

        _usersRepositoryMock.Verify(r =>
            r.SetUserActiveAsync(userId, isActive, CancellationToken.None), Times.Once);
    }
}
using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using UserService.Api.Contracts;
using UserService.Api.Controllers;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.MessageEvents;
using UserService.Application.Abstractions.Services;
using UserService.Application.Models.Authorization;
using UserService.Domain.Models;
using UserService.Infrastructure.Authentication.Tokens.Settings;
using Xunit;

namespace UserService.Tests.ApiTests;

public class UsersControllerTests
{
    private readonly Mock<IUsersService> _usersServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUserEventPublisher> _userEventPublisherMock;
    private readonly Mock<IRolesService> _rolesServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IValidator<RegisterUserRequest>> _registerValidatorMock;
    private readonly Mock<IValidator<LoginUserRequest>> _loginValidatorMock;
    private readonly Mock<IValidator<GetUserByEmailRequest>> _getUserByEmailValidatorMock;
    private readonly Mock<IValidator<PutUserRequest>> _putUserValidatorMock;
    private readonly Mock<IValidator<PostResetPasswordRequest>> _resetPasswordValidatorMock;
    private readonly Mock<IValidator<PostSetNewPasswordRequest>> _setNewPasswordValidatorMock;

    private readonly Mock<IValidator<PostSendAccountConfirmationEmailRequest>>
        _sendAccountConfirmationEmailValidatorMock;

    private readonly Mock<IValidator<PostSetAccountAsConfirmedRequest>> _setAccountAsConfirmedValidatorMock;
    private readonly Mock<IOptions<TokenIdentifiers>> _tokenIdentifiersSettingsMock;
    private readonly Mock<IOptions<AuthorizationRules>> _authorizationRulesMock;

    private readonly UsersController _controller;
    private readonly DefaultHttpContext _httpContext;

    public UsersControllerTests()
    {
        _usersServiceMock = new Mock<IUsersService>();
        _mapperMock = new Mock<IMapper>();
        _userEventPublisherMock = new Mock<IUserEventPublisher>();
        _rolesServiceMock = new Mock<IRolesService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _registerValidatorMock = new Mock<IValidator<RegisterUserRequest>>();
        _loginValidatorMock = new Mock<IValidator<LoginUserRequest>>();
        _getUserByEmailValidatorMock = new Mock<IValidator<GetUserByEmailRequest>>();
        _putUserValidatorMock = new Mock<IValidator<PutUserRequest>>();
        _resetPasswordValidatorMock = new Mock<IValidator<PostResetPasswordRequest>>();
        _setNewPasswordValidatorMock = new Mock<IValidator<PostSetNewPasswordRequest>>();
        _sendAccountConfirmationEmailValidatorMock = new Mock<IValidator<PostSendAccountConfirmationEmailRequest>>();
        _setAccountAsConfirmedValidatorMock = new Mock<IValidator<PostSetAccountAsConfirmedRequest>>();
        _tokenIdentifiersSettingsMock = new Mock<IOptions<TokenIdentifiers>>();
        _authorizationRulesMock = new Mock<IOptions<AuthorizationRules>>();

        
        var authorizationRules = new AuthorizationRules
        {
            RolesWithAdminRights = new List<string> { "Admin" }
        };
        _authorizationRulesMock.Setup(x => x.Value).Returns(authorizationRules);

        
        _controller = new UsersController(
            _usersServiceMock.Object,
            _mapperMock.Object,
            _userEventPublisherMock.Object,
            _tokenIdentifiersSettingsMock.Object,
            _rolesServiceMock.Object,
            _authorizationRulesMock.Object,
            _currentUserServiceMock.Object
        );

        _httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };
    }

    private void SetupAdminUser()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Name, "Admin User"),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.User = principal;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };

        
        _currentUserServiceMock.Setup(x => x.Role).Returns("Admin");
    }

    private void SetupRegularUser()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Name, "Regular User"),
            new Claim(ClaimTypes.Role, "Regular")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.User = principal;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };

        
        _currentUserServiceMock.Setup(x => x.Role).Returns("Regular");
    }

    [Fact]
    public async Task Register_CallsServiceAndReturnsOk()
    {
        
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!"
        };

        _registerValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        
        var result = await _controller.Register(request, _registerValidatorMock.Object, CancellationToken.None);

        
        _usersServiceMock.Verify(s =>
                s.RegisterUserAsync("testuser", "test@example.com", "Password123!", CancellationToken.None),
            Times.Once);

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Login_CallsService_SetsCookiesAndReturnsOk()
    {
        
        var request = new LoginUserRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        var accessToken = "access_token_123";
        var refreshToken = "refresh_token_123";

        var tokenIdentifiers = new TokenIdentifiers
        {
            AccessTokenIdentifier = "access_token",
            RefreshTokenIdentifier = "refresh_token"
        };

        var tokenIdentifiersOptionsMock = new Mock<IOptions<TokenIdentifiers>>();
        tokenIdentifiersOptionsMock.Setup(x => x.Value).Returns(tokenIdentifiers);

        
        var controller = new UsersController(
            _usersServiceMock.Object,
            _mapperMock.Object,
            _userEventPublisherMock.Object,
            tokenIdentifiersOptionsMock.Object,
            _rolesServiceMock.Object,
            _authorizationRulesMock.Object,
            _currentUserServiceMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };

        _loginValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _usersServiceMock
            .Setup(s => s.LoginAsync("test@example.com", "Password123!", CancellationToken.None))
            .ReturnsAsync((accessToken, refreshToken));

        
        var result = await controller.Login(request, _loginValidatorMock.Object, CancellationToken.None);

        
        _usersServiceMock.Verify(s =>
                s.LoginAsync("test@example.com", "Password123!", CancellationToken.None),
            Times.Once);

        var setCookieHeaders = _httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("access_token=access_token_123", setCookieHeaders);
        Assert.Contains("refresh_token=refresh_token_123", setCookieHeaders);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetUsers_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();
        
        var role = Role.CreateInstance("Regular");
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "User1",
                Email = "user1@example.com",
                PasswordHash = "hash",
                Role = role,
                RoleId = role.Id,
                IsConfirmed = true,
                IsActive = true
            }
        };

        var expectedResponse = new List<GetUserResponse>
        {
            new GetUserResponse
            {
                Id = users[0].Id,
                Name = "User1",
                Email = "user1@example.com",
                RoleName = "Regular",
                IsConfirmed = true,
                IsActive = true
            }
        };

        _usersServiceMock
            .Setup(s => s.GetUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        _mapperMock
            .Setup(m => m.Map<List<GetUserResponse>>(users))
            .Returns(expectedResponse);

        
        var result = await _controller.GetUsers(CancellationToken.None);

        
        var okResult = Assert.IsType<ActionResult<List<GetUserResponse>>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(expectedResponse, objectResult.Value);
        _usersServiceMock.Verify(s => s.GetUsersAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetUserById_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();
        var userId = Guid.NewGuid();
        var role = Role.CreateInstance("Regular");
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = role,
            RoleId = role.Id,
            IsConfirmed = true,
            IsActive = true
        };

        var expectedResponse = new GetUserResponse
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            RoleName = "Regular",
            IsConfirmed = true,
            IsActive = true
        };

        _usersServiceMock
            .Setup(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(m => m.Map<GetUserResponse>(user))
            .Returns(expectedResponse);

        
        var result = await _controller.GetUserById(userId, CancellationToken.None);

        
        var okResult = Assert.IsType<ActionResult<GetUserResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(expectedResponse, objectResult.Value);
        _usersServiceMock.Verify(s => s.GetUserByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetUserByEmail_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();
        var request = new GetUserByEmailRequest
        {
            Email = "test@example.com"
        };

        var role = Role.CreateInstance("Regular");
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = role,
            RoleId = role.Id,
            IsConfirmed = true,
            IsActive = true
        };

        var expectedResponse = new GetUserResponse
        {
            Id = user.Id,
            Name = "Test User",
            Email = "test@example.com",
            RoleName = "Regular",
            IsConfirmed = true,
            IsActive = true
        };

        _getUserByEmailValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _usersServiceMock
            .Setup(s => s.GetUserByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mapperMock
            .Setup(m => m.Map<GetUserResponse>(user))
            .Returns(expectedResponse);

        
        var result =
            await _controller.GetUserByEmail(request, CancellationToken.None, _getUserByEmailValidatorMock.Object);

        
        var okResult = Assert.IsType<ActionResult<GetUserResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(expectedResponse, objectResult.Value);
        _usersServiceMock.Verify(s => s.GetUserByEmailAsync("test@example.com", It.IsAny<CancellationToken>()),
            Times.Once);
    }

[Fact]
public async Task UpdateUser_WhenAdmin_CallsServiceAndReturnsOk()
{
    var userId = Guid.NewGuid();
    var request = new PutUserRequest
    {
        Name = "Updated Name",
        Email = "updated@example.com",
        Role = "Admin",
        isConfirmed = true,
        isActive = true
    };

    var authorizationRules = new AuthorizationRules
    {
        RolesWithAdminRights = new List<string> { "Admin" }
    };
    _authorizationRulesMock.Setup(x => x.Value).Returns(authorizationRules);
    
    _currentUserServiceMock.Setup(x => x.Role).Returns("Admin");

    _putUserValidatorMock
        .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
        .ReturnsAsync(new ValidationResult());

    var role = Role.CreateInstance("Admin");
    role.Id = Guid.NewGuid();
    
    _rolesServiceMock
        .Setup(s => s.GetRoleByNameAsync(request.Role, It.IsAny<CancellationToken>()))
        .ReturnsAsync(role);

    _mapperMock
        .Setup(m => m.Map<User>(It.IsAny<object>(), It.IsAny<Action<IMappingOperationOptions<object, User>>>()))
        .Returns(new User
        {
            Id = userId,
            Name = request.Name,
            Email = request.Email,
            Role = role,
            RoleId = role.Id,
            IsConfirmed = request.isConfirmed,
            IsActive = request.isActive
        });

    var result = await _controller.UpdateUser(userId, request, CancellationToken.None, _putUserValidatorMock.Object);

    Assert.IsType<OkResult>(result);
    _usersServiceMock.Verify(s =>
        s.UpdateUserAsync(It.Is<User>(u =>
                u.Id == userId &&
                u.Name == "Updated Name" &&
                u.Email == "updated@example.com" &&
                u.Role.Name == "Admin" &&
                u.IsConfirmed == true &&
                u.IsActive == true),
            It.IsAny<CancellationToken>()), Times.Once);
}
    
    [Fact]
    public async Task ResetPassword_CallsServiceAndReturnsOk()
    {
        
        var request = new PostResetPasswordRequest
        {
            Email = "test@example.com"
        };

        _resetPasswordValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        
        var result =
            await _controller.ResetPassword(request, CancellationToken.None, _resetPasswordValidatorMock.Object);

        
        _usersServiceMock.Verify(s =>
                s.SendResetPasswordRequestAsync("test@example.com", CancellationToken.None),
            Times.Once);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SetNewPassword_CallsServiceAndReturnsOk()
    {
        
        var request = new PostSetNewPasswordRequest
        {
            Token = "reset_token_123",
            NewPassword = "NewPassword123!"
        };

        _setNewPasswordValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        
        var result =
            await _controller.SetNewPassword(request, CancellationToken.None, _setNewPasswordValidatorMock.Object);

        
        _usersServiceMock.Verify(s =>
                s.SetNewPasswordAsync("reset_token_123", "NewPassword123!", CancellationToken.None),
            Times.Once);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeactivateAccount_WhenAdmin_CallsServiceAndEventPublisher_ReturnsOk()
    {
        
        SetupAdminUser();
        var userId = Guid.NewGuid();

        
        var result = await _controller.DeactivateAccount(userId, CancellationToken.None);

        
        _usersServiceMock.Verify(s =>
                s.SetAccountActiveAsync(userId, false, CancellationToken.None),
            Times.Once);
        _userEventPublisherMock.Verify(e =>
                e.UserActivationChanged(userId, false),
            Times.Once);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task ReactivateAccount_WhenAdmin_CallsServiceAndEventPublisher_ReturnsOk()
    {
        
        SetupAdminUser();
        var userId = Guid.NewGuid();

        
        var result = await _controller.ReactivateAccount(userId, CancellationToken.None);

        
        _usersServiceMock.Verify(s =>
                s.SetAccountActiveAsync(userId, true, CancellationToken.None),
            Times.Once);
        _userEventPublisherMock.Verify(e =>
                e.UserActivationChanged(userId, true),
            Times.Once);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Logout_RemovesCookiesAndReturnsOk()
    {
        
        var tokenIdentifiers = new TokenIdentifiers
        {
            AccessTokenIdentifier = "access_token",
            RefreshTokenIdentifier = "refresh_token"
        };

        var tokenIdentifiersOptionsMock = new Mock<IOptions<TokenIdentifiers>>();
        tokenIdentifiersOptionsMock.Setup(x => x.Value).Returns(tokenIdentifiers);

        
        var controller = new UsersController(
            _usersServiceMock.Object,
            _mapperMock.Object,
            _userEventPublisherMock.Object,
            tokenIdentifiersOptionsMock.Object,
            _rolesServiceMock.Object,
            _authorizationRulesMock.Object,
            _currentUserServiceMock.Object
        );

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };

        
        var result = await controller.Logout(CancellationToken.None);

        
        var setCookieHeaders = _httpContext.Response.Headers["Set-Cookie"].ToString();
        Assert.Contains("access_token=; expires=Thu, 01 Jan 1970 00:00:00 GMT", setCookieHeaders);
        Assert.Contains("refresh_token=; expires=Thu, 01 Jan 1970 00:00:00 GMT", setCookieHeaders);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task CheckIfUserIsAuthenticated_ReturnsTrue_WhenUserIsAuthenticated()
    {
        
        SetupRegularUser();

        
        var result = await _controller.CheckIfUserIsAuthenticated();

        
        var okResult = Assert.IsType<Ok<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task CheckIfUserIsAuthenticated_ReturnsFalse_WhenUserIsNotAuthenticated()
    {
        
        _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };

        
        var result = await _controller.CheckIfUserIsAuthenticated();

        
        var okResult = Assert.IsType<Ok<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public void GetCurrentUserRole_ReturnsUserRole_WhenUserIsAuthenticated()
    {
        
        var role = "Admin";
        var userId = Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, "admin@example.com"),
            new Claim(ClaimTypes.Name, "Admin User"),
            new Claim(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _httpContext.User = principal;
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };

        
        var result = _controller.GetCurrentUserRole();

        
        var okResult = Assert.IsType<ActionResult<GetCurrentUserRoleResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        var response = Assert.IsType<GetCurrentUserRoleResponse>(objectResult.Value);
        Assert.Equal(role, response.Role);
    }

    [Fact]
    public async Task SendAccountConfirmationEmail_WhenAuthorized_CallsServiceAndReturnsOk()
    {
        
        SetupRegularUser();
        var request = new PostSendAccountConfirmationEmailRequest
        {
            Email = "test@example.com"
        };

        _sendAccountConfirmationEmailValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        
        var result = await _controller.SendAccountConfirmationEmail(
            request, CancellationToken.None, _sendAccountConfirmationEmailValidatorMock.Object);

        
        _usersServiceMock.Verify(s =>
                s.SendAccountConfirmationEmail("test@example.com", CancellationToken.None),
            Times.Once);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task SetAccountAsConfirmed_WhenAuthorized_CallsServiceAndReturnsOk()
    {
        
        SetupRegularUser();
        var request = new PostSetAccountAsConfirmedRequest
        {
            Token = "confirmation_token_123"
        };

        _setAccountAsConfirmedValidatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        
        var result = await _controller.SetAccountAsConfirmed(
            request, CancellationToken.None, _setAccountAsConfirmedValidatorMock.Object);

        
        _usersServiceMock.Verify(s =>
                s.SetAccountAsConfirmed("confirmation_token_123", CancellationToken.None),
            Times.Once);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task DeleteUser_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();
        var userId = Guid.NewGuid();

        
        var result = await _controller.DeleteUser(userId, CancellationToken.None);

        
        _usersServiceMock.Verify(s =>
                s.DeleteUserAsync(userId, CancellationToken.None),
            Times.Once);
        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task GetUsers_WhenRegularUser_ReturnsForbid()
    {
        
        SetupRegularUser();

        
        var result = await _controller.GetUsers(CancellationToken.None);

        
        var forbidResult = Assert.IsType<ActionResult<List<GetUserResponse>>>(result);
        Assert.IsType<ForbidResult>(forbidResult.Result);
        _usersServiceMock.Verify(s => s.GetUsersAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetUserById_WhenRegularUser_ReturnsForbid()
    {
        
        SetupRegularUser();
        var userId = Guid.NewGuid();

        
        var result = await _controller.GetUserById(userId, CancellationToken.None);

        
        var forbidResult = Assert.IsType<ActionResult<GetUserResponse>>(result);
        Assert.IsType<ForbidResult>(forbidResult.Result);
        _usersServiceMock.Verify(s => s.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
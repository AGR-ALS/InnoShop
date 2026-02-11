using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserService.Api.Contracts;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.MessageEvents;
using UserService.Application.Abstractions.Services;
using UserService.Application.Models.Authorization;
using UserService.Domain.Models;
using UserService.Infrastructure.Authentication.Tokens.Settings;
using UserService.Infrastructure.MessageEvents;

namespace UserService.Api.Controllers;

[ApiController]
[Route("/users")]
public class UsersController : ControllerBase
{
    private readonly IUsersService _usersService;
    private readonly IMapper _mapper;
    private readonly IUserEventPublisher _userEventPublisher;
    private readonly IRolesService _rolesService;
    private readonly ICurrentUserService _currentUserService;
    private readonly AuthorizationRules _authorizationRules;
    private readonly TokenIdentifiers _tokenIdentifiers;

    public UsersController(IUsersService usersService, IMapper mapper, IUserEventPublisher userEventPublisher,
        IOptions<TokenIdentifiers> tokenIdentifiers, IRolesService rolesService,
        IOptions<AuthorizationRules> authorizationRules, ICurrentUserService currentUserService)
    {
        _usersService = usersService;
        _mapper = mapper;
        _userEventPublisher = userEventPublisher;
        _rolesService = rolesService;
        _currentUserService = currentUserService;
        _authorizationRules = authorizationRules.Value;
        _tokenIdentifiers = tokenIdentifiers.Value;
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterUserRequest request,
        [FromServices] IValidator<RegisterUserRequest> validator, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken: cancellationToken);
        await _usersService.RegisterUserAsync(request.Username, request.Email, request.Password, cancellationToken);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginUserRequest loginUserRequest,
        [FromServices] IValidator<LoginUserRequest> validator, CancellationToken cancellationToken)
    {
        await validator.ValidateAndThrowAsync(loginUserRequest, cancellationToken: cancellationToken);
        var (accessToken, refreshToken) =
            await _usersService.LoginAsync(loginUserRequest.Email, loginUserRequest.Password, cancellationToken);
        AddTokenToCookie(_tokenIdentifiers.AccessTokenIdentifier, accessToken);
        AddTokenToCookie(_tokenIdentifiers.RefreshTokenIdentifier, refreshToken);
        return Ok();
    }

    [HttpPost("validate-refresh-token")]
    public async Task<ActionResult> LoginWithRefreshToken([FromServices] IRefreshTokensService refreshTokensService,
        CancellationToken cancellationToken)
    {
        string refreshToken = Request.Cookies[_tokenIdentifiers.RefreshTokenIdentifier] ??
                              throw new UnauthorizedAccessException("Refresh token has expired");
        var accessToken = await _usersService.LoginAsync(refreshToken, cancellationToken);
        AddTokenToCookie(_tokenIdentifiers.AccessTokenIdentifier, accessToken);
        return Ok();
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken)
    {
        RemoveTokenFromCookie(_tokenIdentifiers.AccessTokenIdentifier);
        RemoveTokenFromCookie(_tokenIdentifiers.RefreshTokenIdentifier);
        return Ok();
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<GetUserResponse>>> GetUsers(CancellationToken cancellationToken)
    {
        if(!CheckOnRights())
            return Forbid();
        var users = await _usersService.GetUsersAsync(cancellationToken);
        return Ok(_mapper.Map<List<GetUserResponse>>(users));
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetUserResponse>> GetUserById([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        if(!CheckOnRights())
            return Forbid();
        var user = await _usersService.GetUserByIdAsync(id, cancellationToken);
        return Ok(_mapper.Map<GetUserResponse>(user));
    }

    [Authorize]
    [HttpGet("by-email")]
    public async Task<ActionResult<GetUserResponse>> GetUserByEmail([FromQuery] GetUserByEmailRequest request,
        CancellationToken cancellationToken, [FromServices] IValidator<GetUserByEmailRequest> validator)
    {
        if(!CheckOnRights())
            return Forbid();
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var user = await _usersService.GetUserByEmailAsync(request.Email, cancellationToken);
        return Ok(_mapper.Map<GetUserResponse>(user));
    }


    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateUser([FromRoute] Guid id, [FromBody] PutUserRequest request,
        CancellationToken cancellationToken, [FromServices] IValidator<PutUserRequest> validator)
    {
        if(!CheckOnRights())
            return Forbid();
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        var role = await _rolesService.GetRoleByNameAsync(request.Role, cancellationToken);
        var user = _mapper.Map<User>(request, opt => opt.Items[nameof(Role)] = role);
        user.Id = id;
        user.RoleId = role.Id;
        await _usersService.UpdateUserAsync(user, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteUser([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        if(!CheckOnRights())
            return Forbid();
        await _usersService.DeleteUserAsync(id, cancellationToken);
        return Ok();
    }

    [HttpPost("send-reset-password-mail")]
    public async Task<ActionResult> ResetPassword([FromBody] PostResetPasswordRequest request,
        CancellationToken cancellationToken, [FromServices] IValidator<PostResetPasswordRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        await _usersService.SendResetPasswordRequestAsync(request.Email, cancellationToken);
        return Ok();
    }

    [HttpPut("set-new-password")]
    public async Task<ActionResult> SetNewPassword([FromBody] PostSetNewPasswordRequest request,
        CancellationToken cancellationToken, [FromServices] IValidator<PostSetNewPasswordRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        await _usersService.SetNewPasswordAsync(request.Token, request.NewPassword, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpPost("send-account-confirmation-mail")]
    public async Task<ActionResult> SendAccountConfirmationEmail(
        [FromBody] PostSendAccountConfirmationEmailRequest request,
        CancellationToken cancellationToken, [FromServices] IValidator<PostSendAccountConfirmationEmailRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        await _usersService.SendAccountConfirmationEmail(request.Email, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpPut("confirm-account")]
    public async Task<ActionResult> SetAccountAsConfirmed([FromBody] PostSetAccountAsConfirmedRequest request,
        CancellationToken cancellationToken, [FromServices] IValidator<PostSetAccountAsConfirmedRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        await _usersService.SetAccountAsConfirmed(request.Token, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpPut("{id:guid}/deactivate")]
    public async Task<ActionResult> DeactivateAccount([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        if(!CheckOnRights())
            return Forbid();
        await _usersService.SetAccountActiveAsync(id, false, cancellationToken);
        await _userEventPublisher.UserActivationChanged(id, false);
        return Ok();
    }

    [Authorize]
    [HttpPut("{id:guid}/reactivate")]
    public async Task<ActionResult> ReactivateAccount([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        if(!CheckOnRights())
            return Forbid();
        await _usersService.SetAccountActiveAsync(id, true, cancellationToken);
        await _userEventPublisher.UserActivationChanged(id, true);
        return Ok();
    }

    [HttpGet("isAuth")]
    public async Task<IResult> CheckIfUserIsAuthenticated()
    {
        return Results.Ok(User.Identity != null && (User.Identity.IsAuthenticated));
    }

    [Authorize]
    [HttpGet("role")]
    public ActionResult<GetCurrentUserRoleResponse> GetCurrentUserRole()
    {
        var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

        return Ok(new GetCurrentUserRoleResponse { Role = roleClaim! });
    }

    private void AddTokenToCookie(string tokenName, string token)
    {
        HttpContext.Response.Cookies.Append(tokenName, token);
    }

    private void RemoveTokenFromCookie(string tokenName)
    {
        HttpContext.Response.Cookies.Delete(tokenName);
    }

    private bool CheckOnRights()
    {
        if (_authorizationRules.RolesWithAdminRights.Contains(_currentUserService.Role!))
            return true;
        return false;
    }
}
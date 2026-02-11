using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UserService.Api.Contracts;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Services;
using UserService.Application.Models.Authorization;
using UserService.Application.Services;
using UserService.Domain.Models;

namespace UserService.Api.Controllers;

[Route("/roles")]
public class RolesController : ControllerBase
{
    private readonly IRolesService _rolesService;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;
    private readonly AuthorizationRules _authorizationRules;

    public RolesController(IRolesService rolesService, IMapper mapper, ICurrentUserService currentUserService, IOptions<AuthorizationRules> authorizationRules)
    {
        _rolesService = rolesService;
        _mapper = mapper;
        _currentUserService = currentUserService;
        _authorizationRules = authorizationRules.Value;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<List<GetRoleResponse>>> GetRoles(CancellationToken cancellationToken)
    {
        if(!CheckOnRights())
            return Forbid();
        return Ok(_mapper.Map<List<GetRoleResponse>>(await _rolesService.GetAllRolesAsync(cancellationToken)));
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GetRoleResponse>> GetRoleById([FromRoute]Guid id, CancellationToken cancellationToken)
    {
        if(!CheckOnRights())
            return Forbid();
        return Ok(_mapper.Map<GetRoleResponse>(await _rolesService.GetRoleByIdAsync(id, cancellationToken)));
    }
    
    [Authorize]
    [HttpGet("by-name")]
    public async Task<ActionResult<GetRoleResponse>> GetRoleById([FromQuery]string name, CancellationToken cancellationToken)
    {
        if(!CheckOnRights())
            return Forbid();
        return Ok(_mapper.Map<GetRoleResponse>(await _rolesService.GetRoleByNameAsync(name, cancellationToken)));
    }

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Guid>> CreateRole(PostRoleRequest request, CancellationToken cancellationToken, [FromServices] IValidator<PostRoleRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        if(!CheckOnRights())
            return Forbid();
        var role = _mapper.Map<Role>(request);
        var roleId = await _rolesService.CreateRoleAsync(role, cancellationToken);
        return Ok(roleId);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> UpdateRole([FromRoute] Guid id, PutRoleRequest request,
        CancellationToken cancellationToken, [FromServices] IValidator<PutRoleRequest> validator)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);
        if(!CheckOnRights())
            return Forbid();
        var role = _mapper.Map<Role>(request);
        role.Id = id;
        await _rolesService.UpdateRoleAsync(role, cancellationToken);
        return Ok();
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteRole([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        if(!CheckOnRights())
            return Forbid();
        await _rolesService.DeleteRoleAsync(id, cancellationToken);
        return Ok();
    }
    
    private bool CheckOnRights()
    {
        if (_authorizationRules.RolesWithAdminRights.Contains(_currentUserService.Role!))
            return true;
        return false;
    }
}
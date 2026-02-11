using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using UserService.Api.Contracts;
using UserService.Api.Controllers;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Services;
using UserService.Application.Models.Authorization;
using UserService.Domain.Models;
using Xunit;

namespace UserService.Tests.ApiTests;

public class RolesControllerTests
{
    private readonly Mock<IRolesService> _rolesServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IOptions<AuthorizationRules>> _authorizationRulesMock;
    private readonly RolesController _controller;
    private readonly DefaultHttpContext _httpContext;

    public RolesControllerTests()
    {
        _rolesServiceMock = new Mock<IRolesService>();
        _mapperMock = new Mock<IMapper>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _authorizationRulesMock = new Mock<IOptions<AuthorizationRules>>();

        var authorizationRules = new AuthorizationRules
        {
            RolesWithAdminRights = new List<string> { "Admin" }
        };
        _authorizationRulesMock.Setup(x => x.Value).Returns(authorizationRules);

        _controller = new RolesController(
            _rolesServiceMock.Object,
            _mapperMock.Object,
            _currentUserServiceMock.Object,
            _authorizationRulesMock.Object
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

    [Fact]
    public async Task GetRoles_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();

        var roles = new List<Role>
        {
            Role.CreateInstance("Admin"),
            Role.CreateInstance("Regular")
        };

        var expectedResponse = new List<GetRoleResponse>
        {
            new GetRoleResponse { Id = roles[0].Id, Name = "Admin" },
            new GetRoleResponse { Id = roles[1].Id, Name = "Regular" }
        };

        _rolesServiceMock
            .Setup(s => s.GetAllRolesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(roles);

        _mapperMock
            .Setup(m => m.Map<List<GetRoleResponse>>(roles))
            .Returns(expectedResponse);

        
        var result = await _controller.GetRoles(CancellationToken.None);

        
        var okResult = Assert.IsType<ActionResult<List<GetRoleResponse>>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(expectedResponse, objectResult.Value);
        _rolesServiceMock.Verify(s => s.GetAllRolesAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task GetRoleById_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();
        var roleId = Guid.NewGuid();
        var role = Role.CreateInstance("Admin");
        role.Id = roleId;

        var expectedResponse = new GetRoleResponse
        {
            Id = roleId,
            Name = "Admin"
        };

        _rolesServiceMock
            .Setup(s => s.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _mapperMock
            .Setup(m => m.Map<GetRoleResponse>(role))
            .Returns(expectedResponse);

        
        var result = await _controller.GetRoleById(roleId, CancellationToken.None);

        
        var okResult = Assert.IsType<ActionResult<GetRoleResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(expectedResponse, objectResult.Value);
        _rolesServiceMock.Verify(s => s.GetRoleByIdAsync(roleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetRoleByName_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();
        var roleName = "Admin";
        var role = Role.CreateInstance(roleName);

        var expectedResponse = new GetRoleResponse
        {
            Id = role.Id,
            Name = roleName
        };

        _rolesServiceMock
            .Setup(s => s.GetRoleByNameAsync(roleName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        _mapperMock
            .Setup(m => m.Map<GetRoleResponse>(role))
            .Returns(expectedResponse);

        
        var result = await _controller.GetRoleById(roleName, CancellationToken.None);

        
        var okResult = Assert.IsType<ActionResult<GetRoleResponse>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(expectedResponse, objectResult.Value);
        _rolesServiceMock.Verify(s => s.GetRoleByNameAsync(roleName, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateRole_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();
        var request = new PostRoleRequest
        {
            Name = "NewRole"
        };

        var validatorMock = new Mock<IValidator<PostRoleRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var roleId = Guid.NewGuid();
        var role = Role.CreateInstance(request.Name);
        role.Id = roleId;

        _mapperMock
            .Setup(m => m.Map<Role>(request))
            .Returns(role);

        _rolesServiceMock
            .Setup(s => s.CreateRoleAsync(role, It.IsAny<CancellationToken>()))
            .ReturnsAsync(roleId);

        
        var result = await _controller.CreateRole(request, CancellationToken.None, validatorMock.Object);

        
        var okResult = Assert.IsType<ActionResult<Guid>>(result);
        var objectResult = Assert.IsType<OkObjectResult>(okResult.Result);
        Assert.Equal(roleId, objectResult.Value);
        _rolesServiceMock.Verify(s => s.CreateRoleAsync(role, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRole_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();
        var roleId = Guid.NewGuid();
        var request = new PutRoleRequest
        {
            Name = "UpdatedRole"
        };

        var validatorMock = new Mock<IValidator<PutRoleRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var role = Role.CreateInstance(request.Name);
        role.Id = roleId;

        _mapperMock
            .Setup(m => m.Map<Role>(request))
            .Returns(role);

        
        var result = await _controller.UpdateRole(roleId, request, CancellationToken.None, validatorMock.Object);

        
        Assert.IsType<OkResult>(result);
        _rolesServiceMock.Verify(s =>
            s.UpdateRoleAsync(It.Is<Role>(r =>
                    r.Id == roleId &&
                    r.Name == "UpdatedRole"),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRole_WhenAdmin_CallsServiceAndReturnsOk()
    {
        
        SetupAdminUser();
        var roleId = Guid.NewGuid();

        
        var result = await _controller.DeleteRole(roleId, CancellationToken.None);

        
        Assert.IsType<OkResult>(result);
        _rolesServiceMock.Verify(s => s.DeleteRoleAsync(roleId, CancellationToken.None), Times.Once);
    }
}
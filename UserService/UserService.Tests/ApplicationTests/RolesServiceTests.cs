using Moq;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Services;
using UserService.Domain.Models;
using Xunit;

namespace UserService.Tests.ApplicationTests;

public class RolesServiceTests
{
    private readonly Mock<IRolesRepository> _rolesRepositoryMock;
    private readonly RolesService _rolesService;

    public RolesServiceTests()
    {
        _rolesRepositoryMock = new Mock<IRolesRepository>();
        _rolesService = new RolesService(_rolesRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllRolesAsync_CallsRepository_ReturnsRoles()
    {
        
        var cancellationToken = CancellationToken.None;
        var expectedRoles = new List<Role>
        {
            Role.CreateInstance("Admin"),
            Role.CreateInstance("Regular")
        };

        _rolesRepositoryMock
            .Setup(r => r.GetAllRolesAsync(cancellationToken))
            .ReturnsAsync(expectedRoles);

        
        var result = await _rolesService.GetAllRolesAsync(cancellationToken);

        
        Assert.Equal(expectedRoles, result);
        _rolesRepositoryMock.Verify(r => r.GetAllRolesAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetRoleByIdAsync_CallsRepository_ReturnsRole()
    {
        
        var cancellationToken = CancellationToken.None;
        var roleId = Guid.NewGuid();
        var expectedRole = Role.CreateInstance("Admin");
        expectedRole.Id = roleId;

        _rolesRepositoryMock
            .Setup(r => r.GetRoleByIdAsync(roleId, cancellationToken))
            .ReturnsAsync(expectedRole);

        
        var result = await _rolesService.GetRoleByIdAsync(roleId, cancellationToken);

        
        Assert.Equal(expectedRole, result);
        _rolesRepositoryMock.Verify(r => r.GetRoleByIdAsync(roleId, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task GetRoleByNameAsync_CallsRepository_ReturnsRole()
    {
        
        var cancellationToken = CancellationToken.None;
        var roleName = "Admin";
        var expectedRole = Role.CreateInstance(roleName);

        _rolesRepositoryMock
            .Setup(r => r.GetRoleByNameAsync(roleName, cancellationToken))
            .ReturnsAsync(expectedRole);

        
        var result = await _rolesService.GetRoleByNameAsync(roleName, cancellationToken);

        
        Assert.Equal(expectedRole, result);
        _rolesRepositoryMock.Verify(r => r.GetRoleByNameAsync(roleName, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task CreateRoleAsync_CallsRepository_ReturnsRoleId()
    {
        
        var cancellationToken = CancellationToken.None;
        var roleId = Guid.NewGuid();
        var role = Role.CreateInstance("NewRole");
        role.Id = roleId;

        _rolesRepositoryMock
            .Setup(r => r.CreateRoleAsync(role, cancellationToken))
            .ReturnsAsync(roleId);

        
        var result = await _rolesService.CreateRoleAsync(role, cancellationToken);

        
        Assert.Equal(roleId, result);
        _rolesRepositoryMock.Verify(r => r.CreateRoleAsync(role, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task UpdateRoleAsync_CallsRepository()
    {
        
        var cancellationToken = CancellationToken.None;
        var role = Role.CreateInstance("UpdatedRole");
        role.Id = Guid.NewGuid();

        _rolesRepositoryMock
            .Setup(r => r.UpdateRoleAsync(role, cancellationToken))
            .Returns(Task.CompletedTask);

        
        await _rolesService.UpdateRoleAsync(role, cancellationToken);

        
        _rolesRepositoryMock.Verify(r => r.UpdateRoleAsync(role, cancellationToken), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_CallsRepository()
    {
        
        var cancellationToken = CancellationToken.None;
        var roleId = Guid.NewGuid();

        _rolesRepositoryMock
            .Setup(r => r.DeleteRoleAsync(roleId, cancellationToken))
            .Returns(Task.CompletedTask);

        
        await _rolesService.DeleteRoleAsync(roleId, cancellationToken);

        
        _rolesRepositoryMock.Verify(r => r.DeleteRoleAsync(roleId, cancellationToken), Times.Once);
    }
}
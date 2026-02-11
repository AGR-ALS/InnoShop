using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Moq;
using ProductService.Infrastructure.Users;
using Xunit;

namespace ProductService.Tests.InfrastructureTests.User;

public class CurrentUserServiceTests
{
    [Fact]
    public void Properties_ReturnCorrectValues_WhenUserIsAuthenticated()
    {
        var userId = "12345678-1234-1234-1234-123456789abc";
        var email = "test@example.com";
        var username = "Hello";
        var role = "Admin";
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, username),
            new Claim(ClaimTypes.Role, role)
        }));

        var httpContext = new DefaultHttpContext { User = user };
        var httpContextAccessor = new Mock<IHttpContextAccessor>();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new CurrentUserService(httpContextAccessor.Object);

        Assert.Equal(userId, service.Id);
        Assert.Equal(email, service.Email);
        Assert.Equal(username, service.Username);
        Assert.Equal(role, service.Role);
    }
}
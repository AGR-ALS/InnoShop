using UserService.Application.Abstractions.Authentication;
using UserService.Infrastructure.Authentication.Jwt;
using Xunit;

namespace UserService.Tests.InfrastructureTests.Jwt;

public class PasswordHasherTests
{
    private readonly IPasswordHasher _hasher = new PasswordHasher();
        
    [Theory]
    [InlineData("Password1!")]
    [InlineData("Another$ecret123")]
    [InlineData("VeryLongPasswordWithSpecialChars!@#$%^&*()")]
    public void VerifyHashedPassword_ReturnsTrue_ForCorrectPassword(string password)
    {
        
        var hash = _hasher.HashPassword(password);

        
        var result = _hasher.VerifyHashedPassword(password, hash);

        
        Assert.True(result);
    }

    [Theory]
    [InlineData("Password1!", "WrongPassword")]
    [InlineData("MySecret123", "mysecret123")]
    [InlineData("Test@123", "test@123")]
    public void VerifyHashedPassword_ReturnsFalse_ForIncorrectPassword(string correct, string wrong)
    {
        
        var hash = _hasher.HashPassword(correct);

        
        var result = _hasher.VerifyHashedPassword(wrong, hash);

        
        Assert.False(result);
    }
        
    [Fact]
    public void HashPassword_ProducesDifferentHash_ForSamePassword()
    {
        
        var password = "SamePassword123!";
            
        
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);
            
        
        Assert.NotEqual(hash1, hash2);
            
        
        Assert.True(_hasher.VerifyHashedPassword(password, hash1));
        Assert.True(_hasher.VerifyHashedPassword(password, hash2));
    }
}
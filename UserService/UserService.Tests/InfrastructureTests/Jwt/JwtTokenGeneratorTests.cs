using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using UserService.Domain.Models;
using UserService.Infrastructure.Authentication.Jwt;
using Xunit;

namespace UserService.Tests.InfrastructureTests.Jwt;

public class JwtTokenGeneratorTests
{
    private JwtTokenGenerator CreateGenerator(
            string secret = "d8S5:p@$[vM#>Dgn.xQXWaj&=[:6uRUF",
            string issuer = "http://localhost:5085",
            string audience = "http://localhost:5186",
            int expiryMinutes = 10)
        {
            var settings = new JwtSettings
            {
                SecretKey = secret,
                Issuer = issuer,
                Audience = audience,
                ExpiresInMinutes = expiryMinutes
            };
            return new JwtTokenGenerator(Options.Create(settings));
        }

        [Fact]
        public void GenerateJwtToken_ReturnsValidTokenWithClaims()
        {
            
            var userId = Guid.NewGuid();
            var userName = "testuser";
            var userEmail = "test@example.com";
            var user = new Domain.Models.User
            {
                Id = userId,
                Name = userName,
                Email = userEmail,
                PasswordHash = "hash",
                Role = Role.CreateInstance("Regular"),
                IsConfirmed = true,
                IsActive = true
            };

            var generator = CreateGenerator();

            
            var tokenString = generator.GenerateJwtToken(user);

            
            var handler = new JwtSecurityTokenHandler();
            Assert.True(handler.CanReadToken(tokenString));

            var token = handler.ReadJwtToken(tokenString);

            Assert.Equal("http://localhost:5085", token.Issuer);
            Assert.Contains("http://localhost:5186", token.Audiences);

            var now = DateTime.UtcNow;
            var expectedExpiry = now.AddMinutes(10);
            Assert.True(token.ValidTo > now.AddMinutes(9.9));
            Assert.True(token.ValidTo <= expectedExpiry.AddSeconds(1));

            var claims = token.Claims.ToDictionary(c => c.Type, c => c.Value);
            Assert.Equal(userId.ToString(), claims[ClaimTypes.NameIdentifier]);
            Assert.Equal(userEmail, claims[ClaimTypes.Email]);
            Assert.Equal(userName, claims[ClaimTypes.Name]);
            Assert.Equal("Regular", claims[ClaimTypes.Role]);
        }

        [Fact]
        public void GenerateJwtToken_SignedWithCorrectKey()
        {
            
            var user = new Domain.Models.User
            {
                Id = Guid.NewGuid(),
                Name = "testuser",
                Email = "test@example.com",
                PasswordHash = "hash",
                Role = Role.CreateInstance("Regular"),
                IsConfirmed = true,
                IsActive = true
            };

            var secret = "d8S5:p@$[vM#>Dgn.xQXWaj&=[:6uRUF";
            var generator = CreateGenerator(secret);

            
            var tokenString = generator.GenerateJwtToken(user);

            
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "http://localhost:5085",
                ValidateAudience = true,
                ValidAudience = "http://localhost:5186",
                ValidateLifetime = false, 
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
            };

            var principal = handler.ValidateToken(tokenString, validationParameters, out var validatedToken);
            
            Assert.NotNull(principal);
            Assert.IsType<JwtSecurityToken>(validatedToken);
        }
}
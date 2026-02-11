using UserService.Domain.Models;

namespace UserService.Application.Abstractions.Authentication.Jwt;

public interface IJwtTokenGenerator
{
    string GenerateJwtToken(User user);
}
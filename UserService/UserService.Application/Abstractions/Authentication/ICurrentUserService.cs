namespace UserService.Application.Abstractions.Authentication;

public interface ICurrentUserService
{
    string? Id { get; }
    string? Email { get; }
    string? Username { get; }
    string? Role { get; }
}
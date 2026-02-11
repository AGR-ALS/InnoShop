namespace ProductService.Application.Abstractions.Services;

public interface ICurrentUserService
{
    string? Id { get; }
    string? Email { get; }
    string? Username { get; }
    string? Role { get; }
}
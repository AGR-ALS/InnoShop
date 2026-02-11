namespace UserService.Application.Abstractions.Authentication;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyHashedPassword(string providedPassword, string hashedPassword);
}
namespace UserService.Domain.Models;

public class User
{
    public User()
    {
        
    }
    private User(Guid id, string name, string email, string passwordHash, bool isConfirmed, bool isActive)
    {
        Id = id;
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        IsConfirmed = isConfirmed;
        IsActive = isActive;
    }
    public static User CreateInstance(string name, string email, string passwordHash)
    {
        return new User(Guid.NewGuid(), name, email, passwordHash, false, true);
    }
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public Role Role { get; set; } = null!;
    public Guid RoleId { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsActive { get; set; }
    
}
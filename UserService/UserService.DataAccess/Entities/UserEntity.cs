namespace UserService.DataAccess.Entities;

public class UserEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public RoleEntity Role { get; set; } = null!;
    public bool IsConfirmed { get; set; }
    public bool IsActive { get; set; }
}
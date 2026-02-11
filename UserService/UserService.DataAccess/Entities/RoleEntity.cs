namespace UserService.DataAccess.Entities;

public class RoleEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public List<UserEntity> Users { get; set; } = null!;
}
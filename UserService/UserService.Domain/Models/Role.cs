namespace UserService.Domain.Models;

public class Role
{
    public Role()
    {
        
    }
    private Role(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Role CreateInstance( string name)
    {
        return new Role(Guid.NewGuid(), name);
    }


    public Guid Id { get; set; }
    public string Name { get; set; }
}
namespace UserService.Api.Contracts;

public class GetRoleResponse
{ 
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
}
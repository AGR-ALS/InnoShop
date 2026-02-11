namespace UserService.Api.Contracts;

public class GetUserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string RoleName { get; set; }
    public bool IsConfirmed { get; set; }
    public bool IsActive { get; set; }
}
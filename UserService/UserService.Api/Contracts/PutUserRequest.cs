namespace UserService.Api.Contracts;

public class PutUserRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool isConfirmed { get; set; }
    public bool isActive { get; set; }
}
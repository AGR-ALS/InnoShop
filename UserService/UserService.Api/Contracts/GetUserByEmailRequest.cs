namespace UserService.Api.Contracts;

public class GetUserByEmailRequest
{
    public string Email { get; set; } = null!;
}
namespace UserService.Api.Contracts;

public class PostResetPasswordRequest
{
    public string Email { get; set; } = null!;
}
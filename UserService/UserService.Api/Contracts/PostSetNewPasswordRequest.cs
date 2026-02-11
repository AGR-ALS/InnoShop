namespace UserService.Api.Contracts;

public class PostSetNewPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
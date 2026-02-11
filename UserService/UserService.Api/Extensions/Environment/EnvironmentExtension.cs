namespace UserService.Api.Extensions.Environment;

public static class EnvironmentExtension
{
    public static bool IsDockerEnvironment(this IWebHostEnvironment environment)
    {
        return environment.IsEnvironment("Docker");
    }
}
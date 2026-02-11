using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using ProductService.Infrastructure.Authentication.Jwt;

namespace ProductService.Api.Extensions.Authentication;

public static class AuthenticationExtension
{
    public static void AddAuthenticationWithJwtScheme(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(nameof(JwtSettings)).Get<JwtSettings>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(
            JwtBearerDefaults.AuthenticationScheme,
            options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings!.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings!.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings!.SecretKey)),
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[jwtSettings.TokenIdentifier];
                        return Task.CompletedTask;
                    }
                };
            });
    }
}
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UserService.Api.Extensions;
using UserService.Api.Extensions.Authentication;
using UserService.Api.Extensions.Environment;
using UserService.Api.Mapping;
using UserService.Api.Middleware;
using UserService.Api.Settings;
using UserService.Api.Settings.Cors;
using UserService.Application.Abstractions.Authentication;
using UserService.Application.Abstractions.Authentication.Jwt;
using UserService.Application.Abstractions.MessageEvents;
using UserService.Application.Abstractions.Repositories;
using UserService.Application.Abstractions.Services;
using UserService.Application.Models;
using UserService.Application.Models.Authorization;
using UserService.Application.Services;
using UserService.Application.Settings.Roles;
using UserService.DataAccess;
using UserService.DataAccess.Entities;
using UserService.DataAccess.Repositories;
using UserService.Infrastructure.Authentication;
using UserService.Infrastructure.Authentication.AccountConfirmationTokens;
using UserService.Infrastructure.Authentication.Jwt;
using UserService.Infrastructure.Authentication.RefreshTokens;
using UserService.Infrastructure.Authentication.ResetTokens;
using UserService.Infrastructure.Authentication.Tokens.Settings;
using UserService.Infrastructure.Authentication.Users;
using UserService.Infrastructure.MessageEvents;
using UserService.Infrastructure.MessageEvents.Publishers;
using UserService.Infrastructure.MessageEvents.Settings;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<UserServiceDbContext>();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));

builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.UsingRabbitMq((context, configurator) =>
    {
        var brokerSettings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

        configurator.Host(new Uri(brokerSettings.Host), h =>
        {
            h.Username(brokerSettings.Username);
            h.Password(brokerSettings.Password);
        });
        configurator.ConfigureEndpoints(context);
    });
});


builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<IRefreshTokensRepository, RefreshTokensRepository>();
builder.Services.AddScoped<IRefreshTokensService, RefreshTokensService>();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ISecureTokenGenerator, SecureTokenGenerator>();
builder.Services.AddScoped<IResetTokenRepository, ResetTokenRepository>();
builder.Services.AddScoped<IResetTokenService, ResetTokenService>();
builder.Services.AddScoped<IAccountConfirmationTokenRepository, AccountConfirmationTokenRepository>();
builder.Services.AddScoped<IAccountConfirmationTokenService, AccountConfirmationTokenService>();
builder.Services.AddScoped<IUserEventPublisher, UserEventPublisher>();
builder.Services.AddScoped<IMailEventPublisher, MailEventPublisher>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IRolesRepository, RolesRepository>();
builder.Services.AddScoped<IRolesService, RolesService>();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));
builder.Services.Configure<RefreshTokenSettings>(builder.Configuration.GetSection(nameof(RefreshTokenSettings)));
builder.Services.Configure<DefaultRoleSettings>(builder.Configuration.GetSection(nameof(DefaultRoleSettings)));
builder.Services.Configure<ResetTokenSettings>(builder.Configuration.GetSection(nameof(ResetTokenSettings)));
builder.Services.Configure<AccountConfirmationTokenSettings>(
    builder.Configuration.GetSection(nameof(AccountConfirmationTokenSettings)));
builder.Services.Configure<EmailContents>(builder.Configuration.GetSection(nameof(EmailContents)));
builder.Services.Configure<TokenIdentifiers>(builder.Configuration.GetSection(nameof(TokenIdentifiers)));
builder.Services.Configure<AuthorizationRules>(builder.Configuration.GetSection(nameof(AuthorizationRules)));

builder.Services.AddAutoMapper(cfg => { }, typeof(UserProfile), typeof(UserService.DataAccess.Mapping.UserProfile));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

builder.Services.AddAuthenticationWithJwtScheme(builder.Configuration);
builder.Services.AddAuthorization();


builder.Services.Configure<CorsSettings>(builder.Configuration.GetSection(nameof(CorsSettings)));
var corsSettings = builder.Configuration.GetSection(nameof(CorsSettings)).Get<CorsSettings>();
builder.Services.AddCors(options =>
{
    options.AddPolicy(corsSettings!.PolicyName, policy =>
    {
        policy.WithOrigins(corsSettings.AllowedOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

Console.WriteLine(app.Environment.EnvironmentName);
if (app.Environment.IsDevelopment() || app.Environment.IsDockerEnvironment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();

    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<UserServiceDbContext>();
    db.Database.Migrate();
    if (!db.Users.Any() && !db.Roles.Any())
    {
        var adminRole = new RoleEntity { Id = new Guid("4453f854-8f85-4c48-8608-ac2d276403e3"), Name = "Admin" };
        var regularRole = new RoleEntity { Id = new Guid("975e580b-e4af-405a-9c3a-b2c627364ded"), Name = "Regular" };
        var adminUser = new UserEntity
        {
            Id = new Guid("add53480-6278-4368-be34-9c4d8e2c0830"), Email = "admin@admin.com", IsActive = true,
            IsConfirmed = true,
            PasswordHash = scope.ServiceProvider.GetRequiredService<IPasswordHasher>().HashPassword("admin123"),
            Name = "Admin", RoleId = adminRole.Id, Role = adminRole
        };
        await db.Roles.AddRangeAsync([adminRole, regularRole]);
        await db.Users.AddAsync(adminUser);
        await db.SaveChangesAsync();
    }
}

app.UseCors(corsSettings!.PolicyName);
app.UseMiddleware<ExceptionHandler>();
app.UseHttpsRedirection();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
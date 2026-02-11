using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ProductService.Api.Extensions;
using ProductService.Api.Extensions.Authentication;
using ProductService.Api.Extensions.Environment;
using ProductService.Api.Mapping;
using ProductService.Api.Middleware;
using ProductService.Api.Settings.Application;
using ProductService.Api.Settings.Cors;
using ProductService.Application.Abstractions.Files;
using ProductService.Application.Abstractions.Files.ImageUploading;
using ProductService.Application.Abstractions.Repositories;
using ProductService.Application.Abstractions.Services;
using ProductService.DataAccess;
using ProductService.DataAccess.Repositories;
using ProductService.Infrastructure.Authentication.Jwt;
using ProductService.Infrastructure.Files;
using ProductService.Infrastructure.Files.StaticFiles;
using ProductService.Infrastructure.MessageEvents;
using ProductService.Infrastructure.MessageEvents.Consumers;
using ProductService.Infrastructure.MessageEvents.Settings;
using ProductService.Infrastructure.Users;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StaticFilesSettings>(builder.Configuration.GetSection(nameof(StaticFilesSettings)));
var staticFilesSettings = builder.Configuration.GetSection(nameof(StaticFilesSettings)).Get<StaticFilesSettings>();
builder.WebHost.UseWebRoot(staticFilesSettings?.Path ?? "wwwroot");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<ProductServiceDbContext>();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection(nameof(RabbitMqSettings)));
builder.Services.AddMassTransit(busConfigurator =>
{
    busConfigurator.SetKebabCaseEndpointNameFormatter();
    busConfigurator.AddConsumer<UserActivationChangedConsumer>();
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


builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService.Application.Services.ProductService>();
builder.Services.AddScoped<IImageUploader, ImageUploader>();
builder.Services.AddScoped<IImageUploadingService, ImageUploadingService>();
builder.Services.AddScoped<IFileDeletingService, FileDeletingService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddAutoMapper(cfg => { }, typeof(ProductProfile), typeof(ProductService.DataAccess.Mapping.ProductProfile));
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly, includeInternalTypes: true);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(nameof(JwtSettings)));
builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection(nameof(ApplicationConfiguration)));

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

if (app.Environment.IsDevelopment() || app.Environment.IsDockerEnvironment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ProductServiceDbContext>();
    db.Database.Migrate();
}

app.UseCors(corsSettings!.PolicyName);

app.UseMiddleware<ExceptionHandler>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();


app.Run();

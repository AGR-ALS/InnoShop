using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserService.DataAccess.Configurations;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess;

public class UserServiceDbContext :DbContext
{
    private readonly IConfiguration _configuration;
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
    public DbSet<ResetTokenEntity> ResetTokens { get; set; }
    public DbSet<AccountConfirmationTokenEntity> AccountConfirmationTokens { get; set; }

    public UserServiceDbContext(DbContextOptions<UserServiceDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if(!optionsBuilder.IsConfigured)
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSqlConnectionString"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration(nameof(RefreshTokens)));
        modelBuilder.ApplyConfiguration(new ResetTokenConfiguration(nameof(ResetTokens)));
        modelBuilder.ApplyConfiguration(new AccountConfirmationTokenConfiguration(nameof(AccountConfirmationTokens)));
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(UserServiceDbContext).Assembly);
    }
}
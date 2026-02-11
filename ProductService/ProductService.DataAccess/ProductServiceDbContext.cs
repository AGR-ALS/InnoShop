using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ProductService.DataAccess.Entities;
using Microsoft.Extensions.Configuration;
using ProductService.DataAccess.Configurations;

namespace ProductService.DataAccess;

public class ProductServiceDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public DbSet<ProductEntity> Products { get; set; }

    public ProductServiceDbContext(DbContextOptions<ProductServiceDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(_configuration.GetConnectionString("PostgreSqlConnectionString"));
        }
        
        optionsBuilder.ConfigureWarnings(warnings => 
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning)); //TODO: почему-то при развёртке в докере сервис утверждает, что после миграции были изменения в модели, хотя таковых не было. Поэтому эта строка здесь
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
    }
}
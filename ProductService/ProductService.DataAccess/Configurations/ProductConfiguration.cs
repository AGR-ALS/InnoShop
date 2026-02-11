using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProductService.DataAccess.Entities;

namespace ProductService.DataAccess.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<ProductEntity>
{
    public void Configure(EntityTypeBuilder<ProductEntity> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasMaxLength(36).IsRequired();
        builder.Property(w => w.UserId).HasMaxLength(36).IsRequired();
        builder.Property(w => w.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500).IsRequired(false);
        builder.Property(p => p.Price).HasColumnType("decimal(10,2)").IsRequired();
        builder.Property(p => p.IsAvailable).IsRequired();
        builder.Property(p=>p.CreatedDate).HasDefaultValue(DateOnly.FromDateTime(DateTime.Now)).IsRequired();    
        builder.Property(p=>p.ProductImages).IsRequired(false);
        builder.Property(p=>p.IsOwnerActivated).HasDefaultValue(true).IsRequired();
    }
}
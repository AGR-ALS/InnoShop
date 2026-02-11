using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Name).HasMaxLength(50).IsRequired();
        builder.HasAlternateKey(u=>u.Name);
        builder.HasMany(u => u.Users).WithOne(u => u.Role).HasForeignKey(u => u.RoleId);
    }
}
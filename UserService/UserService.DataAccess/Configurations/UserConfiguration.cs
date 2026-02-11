using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.Application.Abstractions.Authentication;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;
using UserService.Infrastructure.Authentication.Jwt;

namespace UserService.DataAccess.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<UserEntity>
{
    private readonly IPasswordHasher _passwordHasher;

    public UserConfiguration()
    {
        _passwordHasher = new PasswordHasher();
    }

    public void Configure(EntityTypeBuilder<UserEntity> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);
        builder.HasAlternateKey(u => u.Email);
        builder.Property(u => u.Id).ValueGeneratedNever().IsRequired().HasMaxLength(36);
        builder.Property(u => u.Name).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(100).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.IsConfirmed).HasDefaultValue(false).IsRequired();
        builder.Property(u => u.IsActive).HasDefaultValue(false).IsRequired();
        builder.HasOne(u => u.Role).WithMany(u => u.Users).HasForeignKey(u => u.RoleId);
    }
}
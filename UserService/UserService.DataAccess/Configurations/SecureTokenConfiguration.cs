using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Configurations;

public class SecureTokenConfiguration<T> : IEntityTypeConfiguration<T> where T : SecureTokenEntity
{
    private readonly string _tableName;

    public SecureTokenConfiguration(string tableName)
    {
        _tableName = tableName;
    }
    public void Configure(EntityTypeBuilder<T> builder)
    {
        builder.ToTable(_tableName);
        builder.HasKey(x => x.Id);
        builder.Property(x=>x.Id).IsRequired().HasMaxLength(36);
        builder.Property(x => x.Token).IsRequired();
        builder.HasIndex(x => x.Token).IsUnique();
        builder.Property(x => x.UserId).IsRequired().HasMaxLength(36);
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.HasOne(r=>r.User).WithMany().HasForeignKey(r=>r.UserId);
    }
}
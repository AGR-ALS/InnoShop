using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Configurations;

public class RefreshTokenConfiguration(string tableName) : SecureTokenConfiguration<RefreshTokenEntity>(tableName);
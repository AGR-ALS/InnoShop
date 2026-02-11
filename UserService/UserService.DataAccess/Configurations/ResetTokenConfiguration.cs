using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Configurations;

public class ResetTokenConfiguration(string tableName) : SecureTokenConfiguration<ResetTokenEntity>(tableName);
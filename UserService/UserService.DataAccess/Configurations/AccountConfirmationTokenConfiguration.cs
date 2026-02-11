using UserService.DataAccess.Entities;

namespace UserService.DataAccess.Configurations;

public class AccountConfirmationTokenConfiguration(string tableName)
    : SecureTokenConfiguration<AccountConfirmationTokenEntity>(tableName);
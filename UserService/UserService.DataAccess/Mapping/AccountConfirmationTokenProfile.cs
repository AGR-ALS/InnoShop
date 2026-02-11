using AutoMapper;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess.Mapping;

public class AccountConfirmationTokenProfile : Profile
{
    public AccountConfirmationTokenProfile()
    {
        CreateMap<AccountConfirmationToken, AccountConfirmationTokenEntity>();
        CreateMap<AccountConfirmationTokenEntity, AccountConfirmationToken>();
    }
}
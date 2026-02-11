using AutoMapper;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess.Mapping;

public class RefreshTokenProfile:Profile
{
    public RefreshTokenProfile()
    {
        CreateMap<RefreshToken, RefreshTokenEntity>();
        CreateMap<RefreshTokenEntity, RefreshToken>();
    }
}
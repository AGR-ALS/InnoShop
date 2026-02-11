using AutoMapper;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess.Mapping;

public class UserProfile:Profile
{
    public UserProfile()
    {
        CreateMap<User, UserEntity>();
        CreateMap<UserEntity, User>();
        
        CreateMap<Role, RoleEntity>();
        CreateMap<RoleEntity, Role>();
    }
}
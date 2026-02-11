using AutoMapper;
using UserService.Api.Contracts;
using UserService.Domain.Models;

namespace UserService.Api.Mapping;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<Role, GetRoleResponse>();
        CreateMap<PostRoleRequest, Role>();
        CreateMap<PutRoleRequest, Role>();
    }
}
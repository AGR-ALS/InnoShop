using AutoMapper;
using UserService.Api.Contracts;
using UserService.Domain.Models;

namespace UserService.Api.Mapping;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, GetUserResponse>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
        CreateMap<PutUserRequest, User>()
            .ForMember(dest => dest.Role, opt =>
                opt.MapFrom((src, dest, destMember, ctx) =>
                    ctx.Items[nameof(Role)]));;
    }
}
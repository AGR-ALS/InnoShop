using AutoMapper;
using UserService.DataAccess.Entities;
using UserService.Domain.Models;

namespace UserService.DataAccess.Mapping;

public class ResetTokenProfile : Profile
{
    public ResetTokenProfile()
    {
        CreateMap<ResetToken, ResetTokenEntity>();
        CreateMap<ResetTokenEntity, ResetToken>();
    }
}
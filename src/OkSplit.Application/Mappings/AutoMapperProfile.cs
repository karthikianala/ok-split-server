using AutoMapper;
using OkSplit.Application.DTOs.Auth;
using OkSplit.Domain.Entities;

namespace OkSplit.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, UserResponseDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email));
    }
}

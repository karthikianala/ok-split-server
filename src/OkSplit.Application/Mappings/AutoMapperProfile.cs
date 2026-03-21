using AutoMapper;
using OkSplit.Application.DTOs.Auth;
using OkSplit.Application.DTOs.Group;
using OkSplit.Domain.Entities;

namespace OkSplit.Application.Mappings;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Auth
        CreateMap<User, UserResponseDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email));

        // Groups
        CreateMap<Group, GroupResponseDto>()
            .ForMember(d => d.MemberCount, opt => opt.MapFrom(s => s.Members.Count));

        CreateMap<Group, GroupDetailDto>()
            .ForMember(d => d.CreatedByName, opt => opt.MapFrom(s => s.Creator.FullName))
            .ForMember(d => d.Members, opt => opt.MapFrom(s => s.Members));

        CreateMap<GroupMember, MemberDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.User.Email))
            .ForMember(d => d.AvatarUrl, opt => opt.MapFrom(s => s.User.AvatarUrl))
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()));
    }
}

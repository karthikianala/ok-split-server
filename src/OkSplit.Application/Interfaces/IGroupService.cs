using OkSplit.Application.DTOs.Group;

namespace OkSplit.Application.Interfaces;

public interface IGroupService
{
    Task<GroupResponseDto> CreateAsync(Guid userId, CreateGroupDto dto);
    Task<(List<GroupResponseDto> Groups, int TotalCount)> GetUserGroupsAsync(Guid userId, int page, int limit);
    Task<GroupDetailDto> GetDetailAsync(Guid userId, Guid groupId);
    Task<GroupResponseDto> UpdateAsync(Guid userId, Guid groupId, UpdateGroupDto dto);
    Task DeleteAsync(Guid userId, Guid groupId);
    Task<MemberDto> AddMemberAsync(Guid userId, Guid groupId, AddMemberDto dto);
    Task RemoveMemberAsync(Guid userId, Guid groupId, Guid targetUserId);
    Task<MemberDto> UpdateMemberRoleAsync(Guid userId, Guid groupId, Guid targetUserId, UpdateMemberRoleDto dto);
}

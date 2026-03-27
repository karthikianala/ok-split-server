using AutoMapper;
using Microsoft.AspNetCore.Identity;
using OkSplit.Application.DTOs.Group;
using OkSplit.Application.Interfaces;
using OkSplit.Domain.Entities;
using OkSplit.Domain.Enums;
using OkSplit.Domain.Interfaces;

namespace OkSplit.Application.Services;

public class GroupService : IGroupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IMapper _mapper;

    public GroupService(IUnitOfWork unitOfWork, UserManager<User> userManager, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<GroupResponseDto> CreateAsync(Guid userId, CreateGroupDto dto)
    {
        var group = new Group
        {
            Name = dto.Name,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl,
            CreatedBy = userId
        };

        // Auto-add creator as Admin
        group.Members.Add(new GroupMember
        {
            UserId = userId,
            Role = GroupRole.Admin
        });

        await _unitOfWork.Groups.AddAsync(group);
        await _unitOfWork.SaveChangesAsync();

        // Re-fetch to get member count populated
        var created = await _unitOfWork.Groups.GetByIdWithMembersAsync(group.Id);
        return _mapper.Map<GroupResponseDto>(created);
    }

    public async Task<(List<GroupResponseDto> Groups, int TotalCount)> GetUserGroupsAsync(Guid userId, int page, int limit)
    {
        var (groups, totalCount) = await _unitOfWork.Groups.GetUserGroupsAsync(userId, page, limit);
        return (_mapper.Map<List<GroupResponseDto>>(groups), totalCount);
    }

    public async Task<GroupDetailDto> GetDetailAsync(Guid userId, Guid groupId)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");

        EnsureMember(group, userId);
        return _mapper.Map<GroupDetailDto>(group);
    }

    public async Task<GroupResponseDto> UpdateAsync(Guid userId, Guid groupId, UpdateGroupDto dto)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");

        EnsureAdmin(group, userId);

        if (dto.Name != null) group.Name = dto.Name;
        if (dto.Description != null) group.Description = dto.Description;
        if (dto.ImageUrl != null) group.ImageUrl = dto.ImageUrl;
        group.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Groups.Update(group);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<GroupResponseDto>(group);
    }

    public async Task DeleteAsync(Guid userId, Guid groupId)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");

        EnsureAdmin(group, userId);

        // Soft delete
        group.IsActive = false;
        group.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Groups.Update(group);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<MemberDto> AddMemberAsync(Guid userId, Guid groupId, AddMemberDto dto)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");

        EnsureAdmin(group, userId);

        var targetUser = await _userManager.FindByEmailAsync(dto.Email);
        if (targetUser == null)
            throw new KeyNotFoundException("No user found with that email. They need to register first.");

        if (group.Members.Any(m => m.UserId == targetUser.Id))
            throw new ArgumentException("User is already a member of this group.");

        var member = new GroupMember
        {
            GroupId = groupId,
            UserId = targetUser.Id,
            Role = GroupRole.Member
        };

        group.Members.Add(member);
        group.UpdatedAt = DateTime.UtcNow;

        await ActivityLogger.LogAsync(_unitOfWork, groupId, userId,
            "member_joined", "Group", groupId,
            $"{targetUser.FullName} was added to the group");

        await _unitOfWork.SaveChangesAsync();

        return new MemberDto
        {
            UserId = targetUser.Id,
            FullName = targetUser.FullName,
            Email = targetUser.Email!,
            AvatarUrl = targetUser.AvatarUrl,
            Role = GroupRole.Member.ToString(),
            JoinedAt = member.JoinedAt
        };
    }

    public async Task RemoveMemberAsync(Guid userId, Guid groupId, Guid targetUserId)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");

        var member = group.Members.FirstOrDefault(m => m.UserId == targetUserId);
        if (member == null)
            throw new KeyNotFoundException("Member not found in this group.");

        // Creator cannot be removed
        if (targetUserId == group.CreatedBy)
            throw new ArgumentException("The group creator cannot be removed.");

        // Only admins can remove others; members can only remove themselves
        if (userId != targetUserId)
            EnsureAdmin(group, userId);

        var memberName = member.User.FullName;
        group.Members.Remove(member);
        group.UpdatedAt = DateTime.UtcNow;

        await ActivityLogger.LogAsync(_unitOfWork, groupId, userId,
            "member_left", "Group", groupId,
            $"{memberName} was removed from the group");

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<MemberDto> UpdateMemberRoleAsync(Guid userId, Guid groupId, Guid targetUserId, UpdateMemberRoleDto dto)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");

        EnsureAdmin(group, userId);

        var member = group.Members.FirstOrDefault(m => m.UserId == targetUserId);
        if (member == null)
            throw new KeyNotFoundException("Member not found in this group.");

        // Cannot change creator's role
        if (targetUserId == group.CreatedBy)
            throw new ArgumentException("Cannot change the group creator's role.");

        if (!Enum.TryParse<GroupRole>(dto.Role, true, out var newRole))
            throw new ArgumentException("Invalid role. Must be 'Admin' or 'Member'.");

        member.Role = newRole;
        group.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();

        // Re-fetch user info for response
        var user = await _userManager.FindByIdAsync(targetUserId.ToString());
        return new MemberDto
        {
            UserId = targetUserId,
            FullName = user!.FullName,
            Email = user.Email!,
            AvatarUrl = user.AvatarUrl,
            Role = newRole.ToString(),
            JoinedAt = member.JoinedAt
        };
    }

    private static void EnsureMember(Group group, Guid userId)
    {
        if (!group.Members.Any(m => m.UserId == userId))
            throw new UnauthorizedAccessException("You are not a member of this group.");
    }

    private static void EnsureAdmin(Group group, Guid userId)
    {
        var member = group.Members.FirstOrDefault(m => m.UserId == userId);
        if (member == null)
            throw new UnauthorizedAccessException("You are not a member of this group.");
        if (member.Role != GroupRole.Admin)
            throw new UnauthorizedAccessException("Only group admins can perform this action.");
    }
}

using OkSplit.Domain.Entities;

namespace OkSplit.Domain.Interfaces;

public interface IGroupRepository
{
    Task<Group?> GetByIdAsync(Guid id);
    Task<Group?> GetByIdWithMembersAsync(Guid id);
    Task<(List<Group> Groups, int TotalCount)> GetUserGroupsAsync(Guid userId, int page, int limit);
    Task AddAsync(Group group);
    void Update(Group group);
}

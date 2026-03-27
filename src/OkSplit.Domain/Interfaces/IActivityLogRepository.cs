using OkSplit.Domain.Entities;

namespace OkSplit.Domain.Interfaces;

public interface IActivityLogRepository
{
    Task<(List<ActivityLog> Logs, int TotalCount)> GetByGroupAsync(Guid groupId, int page, int limit);
    Task<(List<ActivityLog> Logs, int TotalCount)> GetByUserGroupsAsync(Guid userId, int page, int limit);
    Task AddAsync(ActivityLog log);
}

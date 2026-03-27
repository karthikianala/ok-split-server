using Microsoft.EntityFrameworkCore;
using OkSplit.Domain.Entities;
using OkSplit.Domain.Interfaces;
using OkSplit.Infrastructure.Data;

namespace OkSplit.Infrastructure.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly AppDbContext _context;

    public ActivityLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<ActivityLog> Logs, int TotalCount)> GetByGroupAsync(Guid groupId, int page, int limit)
    {
        var query = _context.Set<ActivityLog>()
            .Where(a => a.GroupId == groupId)
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();
        var logs = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
        return (logs, totalCount);
    }

    public async Task<(List<ActivityLog> Logs, int TotalCount)> GetByUserGroupsAsync(Guid userId, int page, int limit)
    {
        var userGroupIds = _context.GroupMembers
            .Where(gm => gm.UserId == userId)
            .Select(gm => gm.GroupId);

        var query = _context.Set<ActivityLog>()
            .Where(a => userGroupIds.Contains(a.GroupId))
            .Include(a => a.User)
            .Include(a => a.Group)
            .OrderByDescending(a => a.CreatedAt);

        var totalCount = await query.CountAsync();
        var logs = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();
        return (logs, totalCount);
    }

    public async Task AddAsync(ActivityLog log)
    {
        await _context.Set<ActivityLog>().AddAsync(log);
    }
}

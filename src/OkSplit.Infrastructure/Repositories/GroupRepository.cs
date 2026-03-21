using Microsoft.EntityFrameworkCore;
using OkSplit.Domain.Entities;
using OkSplit.Domain.Interfaces;
using OkSplit.Infrastructure.Data;

namespace OkSplit.Infrastructure.Repositories;

public class GroupRepository : IGroupRepository
{
    private readonly AppDbContext _context;

    public GroupRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Group?> GetByIdAsync(Guid id)
    {
        return await _context.Groups
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);
    }

    public async Task<Group?> GetByIdWithMembersAsync(Guid id)
    {
        return await _context.Groups
            .Include(g => g.Members)
                .ThenInclude(m => m.User)
            .Include(g => g.Creator)
            .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);
    }

    public async Task<(List<Group> Groups, int TotalCount)> GetUserGroupsAsync(Guid userId, int page, int limit)
    {
        var query = _context.Groups
            .Where(g => g.IsActive && g.Members.Any(m => m.UserId == userId))
            .Include(g => g.Members)
            .OrderByDescending(g => g.UpdatedAt);

        var totalCount = await query.CountAsync();
        var groups = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (groups, totalCount);
    }

    public async Task AddAsync(Group group)
    {
        await _context.Groups.AddAsync(group);
    }

    public void Update(Group group)
    {
        _context.Groups.Update(group);
    }
}

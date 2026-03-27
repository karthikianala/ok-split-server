using Microsoft.EntityFrameworkCore;
using OkSplit.Domain.Entities;
using OkSplit.Domain.Enums;
using OkSplit.Domain.Interfaces;
using OkSplit.Infrastructure.Data;

namespace OkSplit.Infrastructure.Repositories;

public class SettlementRepository : ISettlementRepository
{
    private readonly AppDbContext _context;

    public SettlementRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Settlement?> GetByIdAsync(Guid id)
    {
        return await _context.Settlements
            .Include(s => s.PaidByUser)
            .Include(s => s.PaidToUser)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Settlement?> GetByIdWithPaymentAsync(Guid id)
    {
        return await _context.Settlements
            .Include(s => s.PaidByUser)
            .Include(s => s.PaidToUser)
            .Include(s => s.Payment)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<(List<Settlement> Settlements, int TotalCount)> GetByGroupAsync(
        Guid groupId, string? status, int page, int limit)
    {
        var query = _context.Settlements
            .Where(s => s.GroupId == groupId)
            .Include(s => s.PaidByUser)
            .Include(s => s.PaidToUser)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<SettlementStatus>(status, true, out var parsedStatus))
            query = query.Where(s => s.Status == parsedStatus);

        query = query.OrderByDescending(s => s.CreatedAt);

        var totalCount = await query.CountAsync();
        var settlements = await query.Skip((page - 1) * limit).Take(limit).ToListAsync();

        return (settlements, totalCount);
    }

    public async Task<List<Settlement>> GetCompletedByGroupAsync(Guid groupId)
    {
        return await _context.Settlements
            .Where(s => s.GroupId == groupId && s.Status == SettlementStatus.Completed)
            .ToListAsync();
    }

    public async Task<List<Settlement>> GetPendingForUserAsync(Guid userId)
    {
        // Settlements where this user is the creditor and needs to confirm
        return await _context.Settlements
            .Where(s => s.Status == SettlementStatus.Pending
                && s.PaidTo == userId
                && s.CreatedByUserId != userId) // Debtor created it, creditor needs to confirm
            .Include(s => s.PaidByUser)
            .Include(s => s.PaidToUser)
            .Include(s => s.Group)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(Settlement settlement)
    {
        await _context.Settlements.AddAsync(settlement);
    }

    public void Update(Settlement settlement)
    {
        _context.Settlements.Update(settlement);
    }
}

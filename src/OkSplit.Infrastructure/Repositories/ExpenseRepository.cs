using Microsoft.EntityFrameworkCore;
using OkSplit.Domain.Entities;
using OkSplit.Domain.Interfaces;
using OkSplit.Infrastructure.Data;

namespace OkSplit.Infrastructure.Repositories;

public class ExpenseRepository : IExpenseRepository
{
    private readonly AppDbContext _context;

    public ExpenseRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Expense?> GetByIdAsync(Guid id)
    {
        return await _context.Expenses.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Expense?> GetByIdWithSplitsAsync(Guid id)
    {
        return await _context.Expenses
            .Include(e => e.Splits)
                .ThenInclude(s => s.User)
            .Include(e => e.PaidByUser)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<(List<Expense> Expenses, int TotalCount)> GetFilteredAsync(
        Guid groupId, string? category, DateTime? startDate, DateTime? endDate,
        Guid? paidBy, int page, int limit)
    {
        var query = _context.Expenses
            .Where(e => e.GroupId == groupId)
            .Include(e => e.PaidByUser)
            .Include(e => e.Splits)
            .AsQueryable();

        if (!string.IsNullOrEmpty(category))
            query = query.Where(e => e.Category.ToString() == category);
        if (startDate.HasValue)
            query = query.Where(e => e.CreatedAt >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(e => e.CreatedAt <= endDate.Value);
        if (paidBy.HasValue)
            query = query.Where(e => e.PaidBy == paidBy.Value);

        query = query.OrderByDescending(e => e.CreatedAt);

        var totalCount = await query.CountAsync();
        var expenses = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync();

        return (expenses, totalCount);
    }

    public async Task<List<Expense>> GetByGroupIdAsync(Guid groupId)
    {
        return await _context.Expenses
            .Where(e => e.GroupId == groupId)
            .Include(e => e.Splits)
            .ToListAsync();
    }

    public async Task AddAsync(Expense expense)
    {
        await _context.Expenses.AddAsync(expense);
    }

    public void Update(Expense expense)
    {
        _context.Expenses.Update(expense);
    }

    public void Delete(Expense expense)
    {
        _context.Expenses.Remove(expense);
    }
}

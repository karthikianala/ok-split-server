using Microsoft.EntityFrameworkCore;
using OkSplit.Application.DTOs.Analytics;
using OkSplit.Application.Interfaces;
using OkSplit.Domain.Enums;
using OkSplit.Domain.Interfaces;

namespace OkSplit.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public AnalyticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<MonthlyBreakdownDto>> GetMonthlyBreakdownAsync(
        Guid userId, Guid? groupId, DateTime startDate, DateTime endDate)
    {
        var expenses = await GetUserExpensesQuery(userId, groupId)
            .Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate)
            .Include(e => e.Splits)
            .ToListAsync();

        var grouped = expenses
            .GroupBy(e => e.CreatedAt.ToString("yyyy-MM"))
            .Select(g => new MonthlyBreakdownDto
            {
                Month = g.Key,
                TotalSpent = g.Sum(e => e.Amount),
                TotalOwed = g.SelectMany(e => e.Splits)
                    .Where(s => s.UserId == userId)
                    .Sum(s => s.OwedAmount),
                TotalPaid = g.Where(e => e.PaidBy == userId).Sum(e => e.Amount)
            })
            .OrderBy(m => m.Month)
            .ToList();

        return grouped;
    }

    public async Task<List<CategorySpendDto>> GetCategorySpendAsync(
        Guid userId, Guid? groupId, DateTime? startDate, DateTime? endDate)
    {
        var query = GetUserExpensesQuery(userId, groupId);

        if (startDate.HasValue) query = query.Where(e => e.CreatedAt >= startDate.Value);
        if (endDate.HasValue) query = query.Where(e => e.CreatedAt <= endDate.Value);

        var expenses = await query.ToListAsync();
        var total = expenses.Sum(e => e.Amount);

        if (total == 0) return new List<CategorySpendDto>();

        return expenses
            .GroupBy(e => e.Category.ToString())
            .Select(g => new CategorySpendDto
            {
                Category = g.Key,
                Amount = g.Sum(e => e.Amount),
                Percentage = Math.Round(g.Sum(e => e.Amount) / total * 100, 1)
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    public async Task<GroupAnalyticsDto> GetGroupSummaryAsync(Guid userId, Guid groupId)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null) throw new KeyNotFoundException("Group not found.");
        if (!group.Members.Any(m => m.UserId == userId))
            throw new UnauthorizedAccessException("You are not a member of this group.");

        var expenses = await _unitOfWork.Expenses.GetByGroupIdAsync(groupId);
        var settlements = await _unitOfWork.Settlements.GetCompletedByGroupAsync(groupId);

        var totalExpenses = expenses.Sum(e => e.Amount);
        var totalSettled = settlements.Sum(s => s.Amount);

        var topSpender = expenses
            .GroupBy(e => e.PaidBy)
            .Select(g => new { UserId = g.Key, Total = g.Sum(e => e.Amount) })
            .OrderByDescending(x => x.Total)
            .FirstOrDefault();

        var topSpenderName = topSpender != null
            ? group.Members.FirstOrDefault(m => m.UserId == topSpender.UserId)?.User.FullName
            : null;

        return new GroupAnalyticsDto
        {
            TotalExpenses = totalExpenses,
            TotalSettled = totalSettled,
            PendingAmount = totalExpenses - totalSettled,
            MemberCount = group.Members.Count,
            TopSpender = topSpenderName,
            TopSpenderAmount = topSpender?.Total ?? 0
        };
    }

    public async Task<PersonalSummaryDto> GetPersonalSummaryAsync(Guid userId)
    {
        // Get all groups user is member of
        var (groups, _) = await _unitOfWork.Groups.GetUserGroupsAsync(userId, 1, 1000);
        var groupIds = groups.Select(g => g.Id).ToList();

        decimal totalOwed = 0;  // Others owe you
        decimal totalOwe = 0;   // You owe others
        int expenseCount = 0;

        foreach (var groupId in groupIds)
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
            if (group == null) continue;

            var expenses = await _unitOfWork.Expenses.GetByGroupIdAsync(groupId);
            var settlements = await _unitOfWork.Settlements.GetCompletedByGroupAsync(groupId);
            var memberNames = group.Members.ToDictionary(m => m.UserId, m => m.User.FullName);

            var balances = DebtSimplificationService.CalculateBalances(expenses, settlements, memberNames);
            var myBalance = balances.FirstOrDefault(b => b.UserId == userId);

            if (myBalance != null)
            {
                if (myBalance.Balance > 0) totalOwed += myBalance.Balance;
                else totalOwe += Math.Abs(myBalance.Balance);
            }

            expenseCount += expenses.Count;
        }

        return new PersonalSummaryDto
        {
            TotalOwed = totalOwed,
            TotalOwe = totalOwe,
            NetBalance = totalOwed - totalOwe,
            GroupCount = groups.Count,
            ExpenseCount = expenseCount
        };
    }

    private IQueryable<Domain.Entities.Expense> GetUserExpensesQuery(Guid userId, Guid? groupId)
    {
        if (groupId.HasValue)
        {
            return _unitOfWork.Expenses.GetByGroupIdAsync(groupId.Value).Result.AsQueryable();
        }

        // All expenses across user's groups — we need to do this via DbContext
        // For now, return empty and handle in callers
        return Enumerable.Empty<Domain.Entities.Expense>().AsQueryable();
    }
}

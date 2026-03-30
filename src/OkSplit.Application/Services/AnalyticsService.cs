using OkSplit.Application.DTOs.Analytics;
using OkSplit.Application.Interfaces;
using OkSplit.Domain.Entities;
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
        var expenses = await GetUserExpensesAsync(userId, groupId);
        expenses = expenses
            .Where(e => e.CreatedAt >= startDate && e.CreatedAt <= endDate)
            .ToList();

        return expenses
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
    }

    public async Task<List<CategorySpendDto>> GetCategorySpendAsync(
        Guid userId, Guid? groupId, DateTime? startDate, DateTime? endDate)
    {
        var expenses = await GetUserExpensesAsync(userId, groupId);

        if (startDate.HasValue) expenses = expenses.Where(e => e.CreatedAt >= startDate.Value).ToList();
        if (endDate.HasValue) expenses = expenses.Where(e => e.CreatedAt <= endDate.Value).ToList();

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
        var (groups, _) = await _unitOfWork.Groups.GetUserGroupsAsync(userId, 1, 1000);

        decimal totalOwed = 0;
        decimal totalOwe = 0;
        int expenseCount = 0;

        foreach (var g in groups)
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(g.Id);
            if (group == null) continue;

            var expenses = await _unitOfWork.Expenses.GetByGroupIdAsync(g.Id);
            var settlements = await _unitOfWork.Settlements.GetCompletedByGroupAsync(g.Id);
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

    private async Task<List<Expense>> GetUserExpensesAsync(Guid userId, Guid? groupId)
    {
        if (groupId.HasValue)
        {
            return await _unitOfWork.Expenses.GetByGroupIdAsync(groupId.Value);
        }

        // Fetch expenses across all user's groups
        var (groups, _) = await _unitOfWork.Groups.GetUserGroupsAsync(userId, 1, 1000);
        var allExpenses = new List<Expense>();

        foreach (var g in groups)
        {
            var expenses = await _unitOfWork.Expenses.GetByGroupIdAsync(g.Id);
            allExpenses.AddRange(expenses);
        }

        return allExpenses;
    }
}

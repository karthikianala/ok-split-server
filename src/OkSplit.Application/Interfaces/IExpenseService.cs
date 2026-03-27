using OkSplit.Application.DTOs.Expense;

namespace OkSplit.Application.Interfaces;

public interface IExpenseService
{
    Task<ExpenseResponseDto> CreateAsync(Guid userId, CreateExpenseDto dto);
    Task<(List<ExpenseResponseDto> Expenses, int TotalCount)> GetFilteredAsync(
        Guid userId, Guid groupId, string? category, DateTime? startDate,
        DateTime? endDate, Guid? paidBy, int page, int limit);
    Task<ExpenseResponseDto> GetDetailAsync(Guid userId, Guid expenseId);
    Task<ExpenseResponseDto> UpdateAsync(Guid userId, Guid expenseId, CreateExpenseDto dto);
    Task DeleteAsync(Guid userId, Guid expenseId);
    Task<List<BalanceDto>> GetGroupBalancesAsync(Guid userId, Guid groupId);
    Task<List<SimplifiedDebtDto>> GetSimplifiedDebtsAsync(Guid userId, Guid groupId);
}

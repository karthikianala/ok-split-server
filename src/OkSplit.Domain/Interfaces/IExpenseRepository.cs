using OkSplit.Domain.Entities;

namespace OkSplit.Domain.Interfaces;

public interface IExpenseRepository
{
    Task<Expense?> GetByIdAsync(Guid id);
    Task<Expense?> GetByIdWithSplitsAsync(Guid id);
    Task<(List<Expense> Expenses, int TotalCount)> GetFilteredAsync(
        Guid groupId, string? category, DateTime? startDate, DateTime? endDate,
        Guid? paidBy, int page, int limit);
    Task<List<Expense>> GetByGroupIdAsync(Guid groupId);
    Task AddAsync(Expense expense);
    void Update(Expense expense);
    void Delete(Expense expense);
}

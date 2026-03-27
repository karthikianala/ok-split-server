using AutoMapper;
using OkSplit.Application.DTOs.Expense;
using OkSplit.Application.Interfaces;
using OkSplit.Domain.Entities;
using OkSplit.Domain.Enums;
using OkSplit.Domain.Interfaces;

namespace OkSplit.Application.Services;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IRealtimeNotifier _notifier;

    public ExpenseService(IUnitOfWork unitOfWork, IMapper mapper, IRealtimeNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notifier = notifier;
    }

    public async Task<ExpenseResponseDto> CreateAsync(Guid userId, CreateExpenseDto dto)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(dto.GroupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");

        EnsureMember(group, userId);

        if (!Enum.TryParse<ExpenseCategory>(dto.Category, true, out var category))
            throw new ArgumentException($"Invalid category: {dto.Category}");
        if (!Enum.TryParse<SplitType>(dto.SplitType, true, out var splitType))
            throw new ArgumentException($"Invalid split type: {dto.SplitType}");

        // Validate all split participants are group members
        var memberIds = group.Members.Select(m => m.UserId).ToHashSet();
        foreach (var split in dto.Splits)
        {
            if (!memberIds.Contains(split.UserId))
                throw new ArgumentException($"User {split.UserId} is not a member of this group.");
        }

        var expense = new Expense
        {
            GroupId = dto.GroupId,
            PaidBy = userId,
            Description = dto.Description,
            Amount = dto.Amount,
            Category = category,
            SplitType = splitType,
            Notes = dto.Notes
        };

        // Calculate splits
        expense.Splits = CalculateSplits(splitType, dto.Amount, dto.Splits);

        await _unitOfWork.Expenses.AddAsync(expense);
        group.UpdatedAt = DateTime.UtcNow;

        await ActivityLogger.LogAsync(_unitOfWork, dto.GroupId, userId,
            "expense_added", "Expense", expense.Id,
            $"{dto.Description} — ₹{dto.Amount}");

        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Expenses.GetByIdWithSplitsAsync(expense.Id);
        var result = _mapper.Map<ExpenseResponseDto>(created);
        await _notifier.NotifyGroupAsync(dto.GroupId, "ExpenseAdded", result);
        return result;
    }

    public async Task<(List<ExpenseResponseDto> Expenses, int TotalCount)> GetFilteredAsync(
        Guid userId, Guid groupId, string? category, DateTime? startDate,
        DateTime? endDate, Guid? paidBy, int page, int limit)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");
        EnsureMember(group, userId);

        var (expenses, totalCount) = await _unitOfWork.Expenses.GetFilteredAsync(
            groupId, category, startDate, endDate, paidBy, page, limit);

        return (_mapper.Map<List<ExpenseResponseDto>>(expenses), totalCount);
    }

    public async Task<ExpenseResponseDto> GetDetailAsync(Guid userId, Guid expenseId)
    {
        var expense = await _unitOfWork.Expenses.GetByIdWithSplitsAsync(expenseId);
        if (expense == null)
            throw new KeyNotFoundException("Expense not found.");

        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(expense.GroupId);
        EnsureMember(group!, userId);

        return _mapper.Map<ExpenseResponseDto>(expense);
    }

    public async Task<ExpenseResponseDto> UpdateAsync(Guid userId, Guid expenseId, CreateExpenseDto dto)
    {
        var expense = await _unitOfWork.Expenses.GetByIdWithSplitsAsync(expenseId);
        if (expense == null)
            throw new KeyNotFoundException("Expense not found.");

        // Only the creator can edit
        if (expense.PaidBy != userId)
            throw new UnauthorizedAccessException("You can only edit expenses you created.");

        if (!Enum.TryParse<ExpenseCategory>(dto.Category, true, out var category))
            throw new ArgumentException($"Invalid category: {dto.Category}");
        if (!Enum.TryParse<SplitType>(dto.SplitType, true, out var splitType))
            throw new ArgumentException($"Invalid split type: {dto.SplitType}");

        expense.Description = dto.Description;
        expense.Amount = dto.Amount;
        expense.Category = category;
        expense.SplitType = splitType;
        expense.Notes = dto.Notes;
        expense.UpdatedAt = DateTime.UtcNow;

        // Recalculate splits — remove old, add new
        expense.Splits.Clear();
        var newSplits = CalculateSplits(splitType, dto.Amount, dto.Splits);
        foreach (var split in newSplits)
            expense.Splits.Add(split);

        _unitOfWork.Expenses.Update(expense);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Expenses.GetByIdWithSplitsAsync(expense.Id);
        return _mapper.Map<ExpenseResponseDto>(updated);
    }

    public async Task DeleteAsync(Guid userId, Guid expenseId)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(expenseId);
        if (expense == null)
            throw new KeyNotFoundException("Expense not found.");

        // Creator can delete their own; admins can delete anyone's
        if (expense.PaidBy != userId)
        {
            var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(expense.GroupId);
            var member = group!.Members.FirstOrDefault(m => m.UserId == userId);
            if (member == null || member.Role != GroupRole.Admin)
                throw new UnauthorizedAccessException("You can only delete your own expenses or be a group admin.");
        }

        await ActivityLogger.LogAsync(_unitOfWork, expense.GroupId, userId,
            "expense_deleted", "Expense", expenseId,
            $"{expense.Description} — ₹{expense.Amount} was deleted");

        var groupId = expense.GroupId;
        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.SaveChangesAsync();
        await _notifier.NotifyGroupAsync(groupId, "ExpenseDeleted", new { expenseId });
    }

    public async Task<List<BalanceDto>> GetGroupBalancesAsync(Guid userId, Guid groupId)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");
        EnsureMember(group, userId);

        var expenses = await _unitOfWork.Expenses.GetByGroupIdAsync(groupId);
        var completedSettlements = await _unitOfWork.Settlements.GetCompletedByGroupAsync(groupId);
        var memberNames = group.Members.ToDictionary(m => m.UserId, m => m.User.FullName);

        return DebtSimplificationService.CalculateBalances(expenses, completedSettlements, memberNames);
    }

    public async Task<List<SimplifiedDebtDto>> GetSimplifiedDebtsAsync(Guid userId, Guid groupId)
    {
        var balances = await GetGroupBalancesAsync(userId, groupId);
        return DebtSimplificationService.SimplifyDebts(balances);
    }

    private static List<ExpenseSplit> CalculateSplits(SplitType splitType, decimal totalAmount, List<SplitInputDto> inputs)
    {
        var splits = new List<ExpenseSplit>();

        switch (splitType)
        {
            case SplitType.Equal:
            {
                var perPerson = Math.Round(totalAmount / inputs.Count, 2);
                // Handle rounding — give remainder to first person
                var remainder = totalAmount - (perPerson * inputs.Count);

                for (int i = 0; i < inputs.Count; i++)
                {
                    splits.Add(new ExpenseSplit
                    {
                        UserId = inputs[i].UserId,
                        OwedAmount = i == 0 ? perPerson + remainder : perPerson
                    });
                }
                break;
            }
            case SplitType.Exact:
            {
                var sum = inputs.Sum(s => s.Amount ?? 0);
                if (Math.Abs(sum - totalAmount) > 0.01m)
                    throw new ArgumentException($"Split amounts ({sum}) must equal the total ({totalAmount}).");

                foreach (var input in inputs)
                {
                    splits.Add(new ExpenseSplit
                    {
                        UserId = input.UserId,
                        OwedAmount = input.Amount ?? 0
                    });
                }
                break;
            }
            case SplitType.Percentage:
            {
                var totalPct = inputs.Sum(s => s.Percentage ?? 0);
                if (Math.Abs(totalPct - 100) > 0.01m)
                    throw new ArgumentException($"Percentages ({totalPct}%) must add up to 100%.");

                foreach (var input in inputs)
                {
                    splits.Add(new ExpenseSplit
                    {
                        UserId = input.UserId,
                        OwedAmount = Math.Round(totalAmount * (input.Percentage ?? 0) / 100, 2)
                    });
                }
                break;
            }
        }

        return splits;
    }

    private static void EnsureMember(Group group, Guid userId)
    {
        if (!group.Members.Any(m => m.UserId == userId))
            throw new UnauthorizedAccessException("You are not a member of this group.");
    }
}

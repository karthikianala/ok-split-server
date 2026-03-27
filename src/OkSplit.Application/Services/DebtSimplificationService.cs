using OkSplit.Application.DTOs.Expense;
using OkSplit.Domain.Entities;

namespace OkSplit.Application.Services;

public static class DebtSimplificationService
{
    /// <summary>
    /// Calculate net balance for each member in a group.
    /// Balance = sum(paid for expenses) - sum(owed from splits) + sum(settlements received) - sum(settlements made)
    /// Positive = is owed money, Negative = owes money.
    /// </summary>
    public static List<BalanceDto> CalculateBalances(
        List<Expense> expenses,
        List<Settlement> completedSettlements,
        Dictionary<Guid, string> memberNames)
    {
        var balances = new Dictionary<Guid, decimal>();

        // Initialize all members with 0
        foreach (var (userId, _) in memberNames)
            balances[userId] = 0;

        // Expenses: payer gets credit, split participants owe
        foreach (var expense in expenses)
        {
            if (balances.ContainsKey(expense.PaidBy))
                balances[expense.PaidBy] += expense.Amount;

            foreach (var split in expense.Splits)
            {
                if (balances.ContainsKey(split.UserId))
                    balances[split.UserId] -= split.OwedAmount;
            }
        }

        // Settlements: debtor's balance goes up (less debt), creditor's balance goes down (less owed)
        foreach (var settlement in completedSettlements)
        {
            if (balances.ContainsKey(settlement.PaidBy))
                balances[settlement.PaidBy] += settlement.Amount;

            if (balances.ContainsKey(settlement.PaidTo))
                balances[settlement.PaidTo] -= settlement.Amount;
        }

        return balances.Select(b => new BalanceDto
        {
            UserId = b.Key,
            FullName = memberNames.GetValueOrDefault(b.Key, "Unknown"),
            Balance = Math.Round(b.Value, 2)
        }).ToList();
    }

    /// <summary>
    /// Greedy algorithm to minimize the number of transactions to settle all debts.
    /// </summary>
    public static List<SimplifiedDebtDto> SimplifyDebts(List<BalanceDto> balances)
    {
        var debts = new List<SimplifiedDebtDto>();

        var creditors = balances
            .Where(b => b.Balance > 0.01m)
            .Select(b => new { b.UserId, b.FullName, Amount = b.Balance })
            .OrderByDescending(c => c.Amount)
            .ToList();

        var debtors = balances
            .Where(b => b.Balance < -0.01m)
            .Select(b => new { b.UserId, b.FullName, Amount = Math.Abs(b.Balance) })
            .OrderByDescending(d => d.Amount)
            .ToList();

        var creditorAmounts = creditors.Select(c => c.Amount).ToArray();
        var debtorAmounts = debtors.Select(d => d.Amount).ToArray();

        int ci = 0, di = 0;
        while (ci < creditors.Count && di < debtors.Count)
        {
            var transfer = Math.Min(creditorAmounts[ci], debtorAmounts[di]);

            if (transfer > 0.01m)
            {
                debts.Add(new SimplifiedDebtDto
                {
                    FromUserId = debtors[di].UserId,
                    FromName = debtors[di].FullName,
                    ToUserId = creditors[ci].UserId,
                    ToName = creditors[ci].FullName,
                    Amount = Math.Round(transfer, 2)
                });
            }

            creditorAmounts[ci] -= transfer;
            debtorAmounts[di] -= transfer;

            if (creditorAmounts[ci] < 0.01m) ci++;
            if (debtorAmounts[di] < 0.01m) di++;
        }

        return debts;
    }
}

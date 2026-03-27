namespace OkSplit.Application.DTOs.Expense;

public class BalanceDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public decimal Balance { get; set; } // +ve = is owed, -ve = owes
}

public class SimplifiedDebtDto
{
    public Guid FromUserId { get; set; }
    public string FromName { get; set; } = string.Empty;
    public Guid ToUserId { get; set; }
    public string ToName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

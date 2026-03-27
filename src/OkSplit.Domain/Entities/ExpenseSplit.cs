namespace OkSplit.Domain.Entities;

public class ExpenseSplit
{
    public Guid Id { get; set; }
    public Guid ExpenseId { get; set; }
    public Guid UserId { get; set; }
    public decimal OwedAmount { get; set; }
    public bool IsSettled { get; set; } = false;

    // Navigation
    public Expense Expense { get; set; } = null!;
    public User User { get; set; } = null!;
}

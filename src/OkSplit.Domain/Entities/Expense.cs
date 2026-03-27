using OkSplit.Domain.Enums;

namespace OkSplit.Domain.Entities;

public class Expense
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid PaidBy { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public ExpenseCategory Category { get; set; }
    public SplitType SplitType { get; set; }
    public string? ReceiptUrl { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Group Group { get; set; } = null!;
    public User PaidByUser { get; set; } = null!;
    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
}

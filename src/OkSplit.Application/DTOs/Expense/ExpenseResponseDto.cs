namespace OkSplit.Application.DTOs.Expense;

public class ExpenseResponseDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid PaidBy { get; set; }
    public string PaidByName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SplitType { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<SplitDetailDto> Splits { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

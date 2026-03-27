namespace OkSplit.Application.DTOs.Expense;

public class CreateExpenseDto
{
    public Guid GroupId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SplitType { get; set; } = string.Empty; // Equal, Exact, Percentage
    public List<SplitInputDto> Splits { get; set; } = new();
    public string? Notes { get; set; }
}

public class SplitInputDto
{
    public Guid UserId { get; set; }
    public decimal? Amount { get; set; }      // For Exact split
    public decimal? Percentage { get; set; }   // For Percentage split
}

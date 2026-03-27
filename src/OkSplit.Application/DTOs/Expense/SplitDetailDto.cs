namespace OkSplit.Application.DTOs.Expense;

public class SplitDetailDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public decimal OwedAmount { get; set; }
    public bool IsSettled { get; set; }
}

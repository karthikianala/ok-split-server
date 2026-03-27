namespace OkSplit.Application.DTOs.Settlement;

public class SettlementResponseDto
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid PaidBy { get; set; }
    public string PaidByName { get; set; } = string.Empty;
    public Guid PaidTo { get; set; }
    public string PaidToName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime? SettledAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

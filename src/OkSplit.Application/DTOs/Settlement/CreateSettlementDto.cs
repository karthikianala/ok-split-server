namespace OkSplit.Application.DTOs.Settlement;

public class CreateSettlementDto
{
    public Guid GroupId { get; set; }
    public Guid PaidBy { get; set; }     // Debtor
    public Guid PaidTo { get; set; }     // Creditor
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Razorpay
}

using OkSplit.Domain.Enums;

namespace OkSplit.Domain.Entities;

public class Settlement
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid PaidBy { get; set; }       // Debtor — person paying
    public Guid PaidTo { get; set; }       // Creditor — person receiving
    public Guid CreatedByUserId { get; set; } // Who recorded this settlement
    public decimal Amount { get; set; }
    public SettlementStatus Status { get; set; } = SettlementStatus.Pending;
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Razorpay
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpayOrderId { get; set; }
    public DateTime? SettledAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Group Group { get; set; } = null!;
    public User PaidByUser { get; set; } = null!;
    public User PaidToUser { get; set; } = null!;
    public Payment? Payment { get; set; }
}

namespace OkSplit.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public Guid SettlementId { get; set; }
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpaySignature { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Status { get; set; } = "Created"; // Created, Authorized, Captured, Failed
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Settlement Settlement { get; set; } = null!;
}

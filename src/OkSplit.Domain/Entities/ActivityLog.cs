namespace OkSplit.Domain.Entities;

public class ActivityLog
{
    public Guid Id { get; set; }
    public Guid GroupId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } = string.Empty;       // expense_added, member_joined, etc.
    public string EntityType { get; set; } = string.Empty;   // Expense, Group, Settlement
    public Guid EntityId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Metadata { get; set; }                     // JSON string
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Group Group { get; set; } = null!;
    public User User { get; set; } = null!;
}

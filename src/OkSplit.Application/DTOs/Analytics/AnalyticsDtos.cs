namespace OkSplit.Application.DTOs.Analytics;

public class MonthlyBreakdownDto
{
    public string Month { get; set; } = string.Empty;  // "2026-03"
    public decimal TotalSpent { get; set; }
    public decimal TotalOwed { get; set; }
    public decimal TotalPaid { get; set; }
}

public class CategorySpendDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class GroupAnalyticsDto
{
    public decimal TotalExpenses { get; set; }
    public decimal TotalSettled { get; set; }
    public decimal PendingAmount { get; set; }
    public int MemberCount { get; set; }
    public string? TopSpender { get; set; }
    public decimal TopSpenderAmount { get; set; }
}

public class PersonalSummaryDto
{
    public decimal TotalOwed { get; set; }     // Others owe you
    public decimal TotalOwe { get; set; }      // You owe others
    public decimal NetBalance { get; set; }
    public int GroupCount { get; set; }
    public int ExpenseCount { get; set; }
}

using OkSplit.Application.DTOs.Analytics;

namespace OkSplit.Application.Interfaces;

public interface IAnalyticsService
{
    Task<List<MonthlyBreakdownDto>> GetMonthlyBreakdownAsync(Guid userId, Guid? groupId, DateTime startDate, DateTime endDate);
    Task<List<CategorySpendDto>> GetCategorySpendAsync(Guid userId, Guid? groupId, DateTime? startDate, DateTime? endDate);
    Task<GroupAnalyticsDto> GetGroupSummaryAsync(Guid userId, Guid groupId);
    Task<PersonalSummaryDto> GetPersonalSummaryAsync(Guid userId);
}

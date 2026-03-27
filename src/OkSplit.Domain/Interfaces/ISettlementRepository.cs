using OkSplit.Domain.Entities;

namespace OkSplit.Domain.Interfaces;

public interface ISettlementRepository
{
    Task<Settlement?> GetByIdAsync(Guid id);
    Task<Settlement?> GetByIdWithPaymentAsync(Guid id);
    Task<(List<Settlement> Settlements, int TotalCount)> GetByGroupAsync(Guid groupId, string? status, int page, int limit);
    Task<List<Settlement>> GetCompletedByGroupAsync(Guid groupId);
    Task<List<Settlement>> GetPendingForUserAsync(Guid userId);
    Task AddAsync(Settlement settlement);
    void Update(Settlement settlement);
}

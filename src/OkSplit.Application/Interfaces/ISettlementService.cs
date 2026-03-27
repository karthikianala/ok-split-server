using OkSplit.Application.DTOs.Settlement;

namespace OkSplit.Application.Interfaces;

public interface ISettlementService
{
    Task<SettlementResponseDto> CreateAsync(Guid userId, CreateSettlementDto dto);
    Task<SettlementResponseDto> ConfirmAsync(Guid userId, Guid settlementId);
    Task<SettlementResponseDto> RejectAsync(Guid userId, Guid settlementId);
    Task<(List<SettlementResponseDto> Settlements, int TotalCount)> GetByGroupAsync(
        Guid userId, Guid groupId, string? status, int page, int limit);
    Task<List<PendingActionDto>> GetPendingActionsAsync(Guid userId);
}

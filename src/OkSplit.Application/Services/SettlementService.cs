using OkSplit.Application.DTOs.Settlement;
using OkSplit.Application.Interfaces;
using OkSplit.Domain.Entities;
using OkSplit.Domain.Enums;
using OkSplit.Domain.Interfaces;

namespace OkSplit.Application.Services;

public class SettlementService : ISettlementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRealtimeNotifier _notifier;

    public SettlementService(IUnitOfWork unitOfWork, IRealtimeNotifier notifier)
    {
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    public async Task<SettlementResponseDto> CreateAsync(Guid userId, CreateSettlementDto dto)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(dto.GroupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");

        // Both paidBy and paidTo must be group members
        if (!group.Members.Any(m => m.UserId == dto.PaidBy))
            throw new ArgumentException("Debtor is not a member of this group.");
        if (!group.Members.Any(m => m.UserId == dto.PaidTo))
            throw new ArgumentException("Creditor is not a member of this group.");

        // Current user must be either the debtor or creditor
        if (userId != dto.PaidBy && userId != dto.PaidTo)
            throw new UnauthorizedAccessException("You can only create settlements you are part of.");

        if (dto.PaidBy == dto.PaidTo)
            throw new ArgumentException("Cannot settle with yourself.");

        if (dto.Amount <= 0)
            throw new ArgumentException("Amount must be positive.");

        var settlement = new Settlement
        {
            GroupId = dto.GroupId,
            PaidBy = dto.PaidBy,
            PaidTo = dto.PaidTo,
            CreatedByUserId = userId,
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod
        };

        // Determine initial status based on who created it and payment method
        if (dto.PaymentMethod.Equals("Razorpay", StringComparison.OrdinalIgnoreCase))
        {
            // Razorpay: only debtor can initiate, stays Pending until payment verified
            if (userId != dto.PaidBy)
                throw new UnauthorizedAccessException("Only the debtor can initiate a Razorpay payment.");
            settlement.Status = SettlementStatus.Pending;
        }
        else if (dto.PaymentMethod.Equals("Cash", StringComparison.OrdinalIgnoreCase))
        {
            if (userId == dto.PaidTo)
            {
                // Creditor recording cash received → auto-complete
                settlement.Status = SettlementStatus.Completed;
                settlement.SettledAt = DateTime.UtcNow;
            }
            else
            {
                // Debtor claiming cash paid → needs creditor confirmation
                settlement.Status = SettlementStatus.Pending;
            }
        }
        else
        {
            throw new ArgumentException("Payment method must be 'Cash' or 'Razorpay'.");
        }

        await _unitOfWork.Settlements.AddAsync(settlement);

        await ActivityLogger.LogAsync(_unitOfWork, dto.GroupId, userId,
            "settlement_created", "Settlement", settlement.Id,
            $"Settlement of ₹{dto.Amount} via {dto.PaymentMethod}");

        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Settlements.GetByIdAsync(settlement.Id);
        return MapToDto(created!);
    }

    public async Task<SettlementResponseDto> ConfirmAsync(Guid userId, Guid settlementId)
    {
        var settlement = await _unitOfWork.Settlements.GetByIdAsync(settlementId);
        if (settlement == null)
            throw new KeyNotFoundException("Settlement not found.");

        if (settlement.Status != SettlementStatus.Pending)
            throw new ArgumentException("Settlement is not pending.");

        // Only the creditor can confirm a cash settlement created by the debtor
        if (settlement.PaymentMethod == "Cash" && settlement.CreatedByUserId != userId && settlement.PaidTo == userId)
        {
            settlement.Status = SettlementStatus.Completed;
            settlement.SettledAt = DateTime.UtcNow;
        }
        else
        {
            throw new UnauthorizedAccessException("You cannot confirm this settlement.");
        }

        _unitOfWork.Settlements.Update(settlement);

        await ActivityLogger.LogAsync(_unitOfWork, settlement.GroupId, userId,
            "settlement_completed", "Settlement", settlementId,
            $"Settlement of ₹{settlement.Amount} confirmed");

        await _unitOfWork.SaveChangesAsync();

        var result = MapToDto(settlement);
        await _notifier.NotifyGroupAsync(settlement.GroupId, "SettlementMade", result);
        return result;
    }

    public async Task<SettlementResponseDto> RejectAsync(Guid userId, Guid settlementId)
    {
        var settlement = await _unitOfWork.Settlements.GetByIdAsync(settlementId);
        if (settlement == null)
            throw new KeyNotFoundException("Settlement not found.");

        if (settlement.Status != SettlementStatus.Pending)
            throw new ArgumentException("Settlement is not pending.");

        // Only the creditor can reject
        if (settlement.PaidTo != userId)
            throw new UnauthorizedAccessException("Only the creditor can reject this settlement.");

        settlement.Status = SettlementStatus.Rejected;
        _unitOfWork.Settlements.Update(settlement);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(settlement);
    }

    public async Task<(List<SettlementResponseDto> Settlements, int TotalCount)> GetByGroupAsync(
        Guid userId, Guid groupId, string? status, int page, int limit)
    {
        var group = await _unitOfWork.Groups.GetByIdWithMembersAsync(groupId);
        if (group == null)
            throw new KeyNotFoundException("Group not found.");
        if (!group.Members.Any(m => m.UserId == userId))
            throw new UnauthorizedAccessException("You are not a member of this group.");

        var (settlements, totalCount) = await _unitOfWork.Settlements.GetByGroupAsync(groupId, status, page, limit);
        return (settlements.Select(MapToDto).ToList(), totalCount);
    }

    public async Task<List<PendingActionDto>> GetPendingActionsAsync(Guid userId)
    {
        var pending = await _unitOfWork.Settlements.GetPendingForUserAsync(userId);
        return pending.Select(s => new PendingActionDto
        {
            SettlementId = s.Id,
            GroupId = s.GroupId,
            GroupName = s.Group.Name,
            PaidByName = s.PaidByUser.FullName,
            Amount = s.Amount,
            PaymentMethod = s.PaymentMethod,
            CreatedAt = s.CreatedAt
        }).ToList();
    }

    private static SettlementResponseDto MapToDto(Settlement s)
    {
        return new SettlementResponseDto
        {
            Id = s.Id,
            GroupId = s.GroupId,
            PaidBy = s.PaidBy,
            PaidByName = s.PaidByUser.FullName,
            PaidTo = s.PaidTo,
            PaidToName = s.PaidToUser.FullName,
            Amount = s.Amount,
            Status = s.Status.ToString(),
            PaymentMethod = s.PaymentMethod,
            SettledAt = s.SettledAt,
            CreatedAt = s.CreatedAt
        };
    }
}

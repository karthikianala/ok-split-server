using OkSplit.Domain.Entities;
using OkSplit.Domain.Interfaces;

namespace OkSplit.Application.Services;

public static class ActivityLogger
{
    public static async Task LogAsync(
        IUnitOfWork unitOfWork, Guid groupId, Guid userId,
        string action, string entityType, Guid entityId, string description,
        string? metadata = null)
    {
        await unitOfWork.ActivityLogs.AddAsync(new ActivityLog
        {
            GroupId = groupId,
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Description = description,
            Metadata = metadata
        });
    }
}

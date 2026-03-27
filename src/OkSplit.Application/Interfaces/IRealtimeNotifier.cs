namespace OkSplit.Application.Interfaces;

public interface IRealtimeNotifier
{
    Task NotifyGroupAsync(Guid groupId, string eventName, object data);
}

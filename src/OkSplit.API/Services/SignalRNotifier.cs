using Microsoft.AspNetCore.SignalR;
using OkSplit.API.Hubs;
using OkSplit.Application.Interfaces;

namespace OkSplit.API.Services;

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<ExpenseHub> _hubContext;

    public SignalRNotifier(IHubContext<ExpenseHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyGroupAsync(Guid groupId, string eventName, object data)
    {
        await _hubContext.Clients.Group(groupId.ToString()).SendAsync(eventName, data);
    }
}

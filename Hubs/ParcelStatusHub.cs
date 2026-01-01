using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace CargoParcelTracker.Hubs;

/// <summary>
/// SignalR hub for real-time parcel status updates
/// Demonstrates .NET's built-in WebSocket support - superior to Django Channels
/// </summary>
[Authorize]
public class ParcelStatusHub : Hub
{
    private readonly ILogger<ParcelStatusHub> _logger;

    public ParcelStatusHub(ILogger<ParcelStatusHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userName = Context.User?.Identity?.Name ?? "Anonymous";
        _logger.LogInformation("Client connected: {ConnectionId} - User: {UserName}", Context.ConnectionId, userName);

        await Groups.AddToGroupAsync(Context.ConnectionId, "ParcelUpdates");
        await Clients.Caller.SendAsync("Connected", $"Welcome {userName}! You're now receiving real-time parcel updates.");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userName = Context.User?.Identity?.Name ?? "Anonymous";
        _logger.LogInformation("Client disconnected: {ConnectionId} - User: {UserName}", Context.ConnectionId, userName);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "ParcelUpdates");
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Broadcast parcel status change to all connected clients
    /// </summary>
    public async Task BroadcastParcelStatusChange(int parcelId, string parcelName, string oldStatus, string newStatus, string userName)
    {
        _logger.LogInformation("Broadcasting status change: Parcel {ParcelId} ({ParcelName}) changed from {OldStatus} to {NewStatus} by {UserName}",
            parcelId, parcelName, oldStatus, newStatus, userName);

        await Clients.Group("ParcelUpdates").SendAsync("ParcelStatusChanged", new
        {
            parcelId,
            parcelName,
            oldStatus,
            newStatus,
            userName,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notify clients when a new parcel is created
    /// </summary>
    public async Task BroadcastNewParcel(int parcelId, string parcelName, string crudeGrade, decimal quantity, string userName)
    {
        _logger.LogInformation("Broadcasting new parcel: {ParcelId} ({ParcelName}) created by {UserName}",
            parcelId, parcelName, userName);

        await Clients.Group("ParcelUpdates").SendAsync("NewParcelCreated", new
        {
            parcelId,
            parcelName,
            crudeGrade,
            quantity,
            userName,
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Notify clients when a parcel is deleted
    /// </summary>
    public async Task BroadcastParcelDeleted(int parcelId, string parcelName, string userName)
    {
        _logger.LogInformation("Broadcasting parcel deletion: {ParcelId} ({ParcelName}) deleted by {UserName}",
            parcelId, parcelName, userName);

        await Clients.Group("ParcelUpdates").SendAsync("ParcelDeleted", new
        {
            parcelId,
            parcelName,
            userName,
            timestamp = DateTime.UtcNow
        });
    }
}

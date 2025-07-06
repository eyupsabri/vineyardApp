using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace VineyardApp.Hubs
{
    [Authorize]
    public class PumpStatusHub : Hub
    {
        // Called by client to start listening to a specific device:
        public Task JoinDeviceGroup(string deviceId) =>
          Groups.AddToGroupAsync(Context.ConnectionId, deviceId);

        public Task LeaveDeviceGroup(string deviceId) =>
          Groups.RemoveFromGroupAsync(Context.ConnectionId, deviceId);

        public override async Task OnConnectedAsync()
        {
            // optional: auto-join via query param
            var devices = Context.GetHttpContext()?
                                 .Request.Query["devices"]
                                 .ToString()
                                 .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                 ?? Array.Empty<string>();

            foreach (var id in devices)
                await Groups.AddToGroupAsync(Context.ConnectionId, id);

            await base.OnConnectedAsync();
        }
    }
}

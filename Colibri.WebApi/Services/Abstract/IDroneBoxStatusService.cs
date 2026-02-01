using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Colibri.WebApi.Services.Abstract;

public interface IDroneBoxStatusService
{
        Task CheckDroneBoxStatusAsync();
        void AddConnection(WebSocket webSocket);
        void RemoveConnection(WebSocket webSocket);
        Task SendStatusToAllAsync(object status);
}

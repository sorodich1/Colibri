using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Colibri.WebApi.Services.Abstract;

public interface IWebSocketStatusService
{
        Task SendStatusToAllAsync(string status);
        void AddConnection(WebSocket webSocket);
        void RemoveConnection(WebSocket webSocket);
        Task CheckDroneConnectionAsync();
}

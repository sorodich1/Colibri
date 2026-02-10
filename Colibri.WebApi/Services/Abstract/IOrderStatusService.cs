using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Colibri.WebApi.Services.Abstract;

public interface IOrderStatusService
{
    void AddConnection(WebSocket webSocket);
    void RemoveConnection(WebSocket webSocket);
    Task NotifyAllClientsAsync();
    Task NotifyOrderUpdateAsync(int orderId, string status, object additionalData = null);
    Task SubscribeToOrder(WebSocket webSocket, int orderId);
    Task UnsubscribeFromOrder(WebSocket webSocket, int orderId);
    Task GetOrderUpdatesAsync(WebSocket webSocket);
}

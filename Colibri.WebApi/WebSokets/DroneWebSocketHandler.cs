using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Colibri.WebApi.Models;
using Microsoft.AspNetCore.Http;

namespace Colibri.WebApi.WebSokets;

public class DroneWebSocketHandler
{
    private static readonly ConcurrentDictionary<string, WebSocket> _connection = new();
    private static readonly ConcurrentDictionary<string, string> _droneSubscriptions = new();

    public async Task HandleWebSocketConnection(HttpContext context)
    {
        var webSokets = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();

        _connection[connectionId] = webSokets;

        try
        {
            await HandleWebSocketMessages(webSokets, connectionId);
        }
        finally
        {
            _connection.TryRemove(connectionId, out _);
            RemoveSubscription(connectionId);
        }
    }

    private async Task HandleWebSocketMessages(WebSocket webSocket, string connectionId)
    {
        var buffer = new byte[1024 * 4];

        while (webSocket.State == WebSocketState.Open)
        {
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                await ProcessClientMessage(message, connectionId);

            }
            else if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                break;
            }
        }
    }

    private async Task ProcessClientMessage(string message, string connectionId)
    {
        try
        {
            var messageObj = JsonSerializer.Deserialize<WebSocketMessage>(message);

            if (messageObj.Type == "subscribe" && messageObj.DroneId != null)
            {
                _droneSubscriptions[connectionId] = messageObj.DroneId;
                await SendToConnect(connectionId, new { type = "subscribed", droneId = messageObj.DroneId });
            }
            else if (messageObj.Type == "unsubscribe")
            {
                RemoveSubscription(connectionId);
                await SendToConnect(connectionId, new { type = "unsubscribed" });
            }
        }
        catch (Exception ex)
        {
            await SendToConnect(connectionId, new { type = "error", message = ex.Message });
        }
    }

    private void RemoveSubscription(string connectionId)
    {
        _droneSubscriptions.TryRemove(connectionId, out _);
    }

    private async Task SendToConnect(string connectionId, object message)
    {
        if (_connection.TryGetValue(connectionId, out var webSoket) && webSoket.State == WebSocketState.Open)
        {
            var jsonMessage = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(jsonMessage);
            await webSoket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    //Для всех подписчиков
    public async Task BroadcastDroneStatus(string droneId, object statusUpdate)
    {
        var message = new
        {
            type = "status_update",
            droneId = droneId,
            data = statusUpdate,
            timeSpan = DateTime.UtcNow
        };

        var tasks = new List<Task>();

        foreach (var subscription in _droneSubscriptions)
        {
            if (subscription.Value == droneId && _connection.TryGetValue(subscription.Key, out var webSoket))
            {
                if (webSoket.State == WebSocketState.Open)
                {
                    tasks.Add(SendToConnect(subscription.Key, message));
                }
            }
        }
        await Task.WhenAll(tasks);
    }
}

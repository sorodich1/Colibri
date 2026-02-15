using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Colibri.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class OrderWebSocketHandler(OrderStatusService orderStatus, ILogger<OrderWebSocketHandler> logger)
{
    private readonly OrderStatusService _orderStatus = orderStatus;
    private readonly ILogger<OrderWebSocketHandler> _logger = logger;

    public async Task HandleWebSocketConnection(HttpContext context)
    {
        try
        {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            _orderStatus.AddConnection(webSocket);
            _logger.LogInformation("Order WebSocket connection accepted");

            await _orderStatus.NotifyAllClientsAsync();
            await HandleWebSocketMessages(webSocket);
        }
        catch (Exception ex)
        {
            // ✅ ИСПРАВЛЕНО
            _logger.LogError(ex, "Error in Order WebSocket connection");
        }
    }

    private async Task HandleWebSocketMessages(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Connection closed",
                        CancellationToken.None);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    _logger.LogDebug("Received Order WebSocket message: {Message}", message);
                    await HandleClientMessage(message, webSocket);
                }
            }
        }
        finally
        {
            _orderStatus.RemoveConnection(webSocket);
            webSocket?.Dispose();
        }
    }

    private async Task HandleClientMessage(string message, WebSocket webSocket)
    {
        try
        {
            var messageObj = JsonSerializer.Deserialize<OrderWebSocketMessage>(message);

            if (messageObj != null)
            {
                switch (messageObj.Type)
                {
                    case "get_status":
                        await _orderStatus.NotifyAllClientsAsync();
                        break;

                    case "subscribe_order":
                        if (messageObj.Data.TryGetProperty("orderId", out var orderIdElement))
                        {
                            var orderId = orderIdElement.GetInt32();
                            await _orderStatus.SubscribeToOrder(webSocket, orderId);
                        }
                        break;

                    case "unsubscribe_order":
                        if (messageObj.Data.TryGetProperty("orderId", out var orderIdElement2))
                        {
                            var orderId = orderIdElement2.GetInt32();
                            await _orderStatus.UnsubscribeFromOrder(webSocket, orderId);
                        }
                        break;

                    case "get_order_updates":
                        await _orderStatus.GetOrderUpdatesAsync(webSocket);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка обработки сообщения клиента Order");
        }
    }

    private class OrderWebSocketMessage
    {
        public string Type { get; set; }
        public JsonElement Data { get; set; }
    }
}
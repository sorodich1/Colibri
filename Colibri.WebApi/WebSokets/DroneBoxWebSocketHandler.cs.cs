// DroneBoxWebSocketHandler.cs
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Colibri.WebApi.Services.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Colibri.WebApi.WebSokets
{
    public class DroneBoxWebSocketHandler
    {
        private readonly IDroneBoxStatusService _statusService;
        private readonly ILogger<DroneBoxWebSocketHandler> _logger;

        public DroneBoxWebSocketHandler(
            IDroneBoxStatusService statusService, 
            ILogger<DroneBoxWebSocketHandler> logger)
        {
            _statusService = statusService;
            _logger = logger;
        }

        public async Task HandleWebSocketConnection(HttpContext context)
        {
            try
            {
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                _statusService.AddConnection(webSocket);
                _logger.LogInformation("DroneBox WebSocket connection accepted");

                // Сразу отправляем статус дронбокса при подключении
                await _statusService.CheckDroneBoxStatusAsync();

                // Обрабатываем сообщения от клиента
                await HandleWebSocketMessages(webSocket);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in DroneBox WebSocket connection: {ex.Message}");
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

                    // Обработка входящих сообщений
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogDebug($"Received DroneBox WebSocket message: {message}");

                        await HandleClientMessage(message, webSocket);
                    }
                }
            }
            finally
            {
                _statusService.RemoveConnection(webSocket);
                webSocket?.Dispose();
            }
        }

        private async Task HandleClientMessage(string message, WebSocket webSocket)
        {
            try
            {
                if (message.Contains("\"type\":\"get_status\""))
                {
                    await _statusService.CheckDroneBoxStatusAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling DroneBox client message: {ex.Message}");
            }
        }
    }
}
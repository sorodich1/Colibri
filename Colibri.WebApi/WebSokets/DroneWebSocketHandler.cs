// DroneWebSocketHandler.cs
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
    public class DroneWebSocketHandler
    {
        private readonly IWebSocketStatusService _statusService;
        private readonly ILogger<DroneWebSocketHandler> _logger;

        public DroneWebSocketHandler(IWebSocketStatusService statusService, ILogger<DroneWebSocketHandler> logger)
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
                _logger.LogInformation("WebSocket connection accepted");

                // Отправляем текущий статус сразу после подключения
                await _statusService.CheckDroneConnectionAsync();

                // Обрабатываем сообщения от клиента (если нужно)
                await HandleWebSocketMessages(webSocket);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in WebSocket connection: {ex.Message}");
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

                    // Обработка входящих сообщений (если нужно)
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogDebug($"Received WebSocket message: {message}");

                        // Можно обрабатывать команды от клиента
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
                // Пример: клиент может запросить текущий статус
                if (message.Contains("\"type\":\"get_status\""))
                {
                    await _statusService.CheckDroneConnectionAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error handling client message: {ex.Message}");
            }
        }
    }
}
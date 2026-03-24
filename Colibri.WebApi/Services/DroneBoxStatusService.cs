// DroneBoxStatusService.cs
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Logging;

namespace Colibri.WebApi.Services
{
    public class DroneBoxStatusService : IDroneBoxStatusService
    {
        private readonly ConcurrentDictionary<WebSocket, byte> _connectedSockets = new();
        private readonly ILogger<DroneBoxStatusService> _logger;
        private readonly IPingService _pingService;
        private const string DRONE_BOX_IP = "37.29.71.91"; // "37.29.40.50"; 

        public DroneBoxStatusService(
            ILogger<DroneBoxStatusService> logger,
            IPingService pingService)
        {
            _logger = logger;
            _pingService = pingService;
        }

        public async Task CheckDroneBoxStatusAsync()
        {
            try
            {
               // _logger.LogInformation($"Checking drone box status at {DRONE_BOX_IP}");

                // Проверяем доступность дронбокса
                bool isOnline = await _pingService.PingHostAsync(DRONE_BOX_IP);

                // Отправляем статус всем подключенным клиентам
                await SendStatusToAllAsync(new
                {
                    ip = DRONE_BOX_IP,
                    online = isOnline,
                    timestamp = DateTime.UtcNow,
                    lastCheck = DateTime.Now.ToString("HH:mm:ss")
                });

                _logger.LogInformation($"Drone box {DRONE_BOX_IP} is {(isOnline ? "🟢 ONLINE" : "🔴 OFFLINE")}");
            }
            catch (Exception ex)
            {
               // _logger.LogError($"Error checking drone box status: {ex.Message}");
                
                // Отправляем сообщение об ошибке
                await SendStatusToAllAsync(new
                {
                    ip = DRONE_BOX_IP,
                    online = false,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        }

        public void AddConnection(WebSocket webSocket)
        {
            _connectedSockets.TryAdd(webSocket, 0);
            _logger.LogInformation($"DroneBox WebSocket connection added. Total connections: {_connectedSockets.Count}");
        }

        public void RemoveConnection(WebSocket webSocket)
        {
            _connectedSockets.TryRemove(webSocket, out _);
            _logger.LogInformation($"DroneBox WebSocket connection removed. Total connections: {_connectedSockets.Count}");
        }

        public async Task SendStatusToAllAsync(object status)
        {
            var message = JsonSerializer.Serialize(status);
            var buffer = Encoding.UTF8.GetBytes(message);
            var segment = new ArraySegment<byte>(buffer);

            foreach (var socket in _connectedSockets.Keys)
            {
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await socket.SendAsync(segment,
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to send to WebSocket: {ex.Message}");
                    }
                }
            }
        }
    }
}
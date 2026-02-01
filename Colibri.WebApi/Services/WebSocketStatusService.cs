using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Colibri.WebApi.Services;

public class WebSocketStatusService(IDroneConnectionService droneConnectionService, ILogger<WebSocketStatusService> logger) : IWebSocketStatusService, IHostedService, IDisposable
{
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly IDroneConnectionService _droneConnectionService = droneConnectionService;
        private readonly ILogger<WebSocketStatusService> _logger = logger;
        private Timer _statusTimer;

    public void AddConnection(WebSocket webSocket)
    {
        var connectionId = Guid.NewGuid().ToString();
        _connections.TryAdd(connectionId, webSocket);
        _logger.LogInformation($"WebSocket connection added. Total connections: {_connections.Count}");
    }

    public async Task CheckDroneConnectionAsync()
    {
        try
        {
            // Проверяем соединение с дроном
            var result = await _droneConnectionService.SendCommandToDrone("status", null);
                
            var status = result.Success ? "connected" : "disconnected";
                
                // Отправляем статус всем подключенным клиентам
            await SendStatusToAllAsync(status);
                
            _logger.LogInformation($"Drone connection status: {status}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking drone connection: {ex.Message}");
            await SendStatusToAllAsync("error");
        }
    }

    public void Dispose()
    {
        _statusTimer?.Dispose();
    }

    public void RemoveConnection(WebSocket webSocket)
    {
        foreach (var kvp in _connections)
        {
            if (kvp.Value == webSocket)
            {
                _connections.TryRemove(kvp.Key, out _);
                _logger.LogInformation($"WebSocket connection removed. Total connections: {_connections.Count}");
                break;
            }
        }
    }

    public async Task SendStatusToAllAsync(string status)
    {
            var message = $"{{\"type\":\"connection_status\",\"status\":\"{status}\",\"timestamp\":\"{DateTime.UtcNow:o}\"}}";
            var buffer = Encoding.UTF8.GetBytes(message);

            foreach (var kvp in _connections.ToArray())
            {
                if (kvp.Value.State == WebSocketState.Open)
                {
                    try
                    {
                        await kvp.Value.SendAsync(
                            new ArraySegment<byte>(buffer),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Failed to send to connection {kvp.Key}: {ex.Message}");
                        RemoveConnection(kvp.Value);
                    }
                }
                else
                {
                    RemoveConnection(kvp.Value);
                }
            }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
            // Запускаем таймер для периодической проверки статуса
            _statusTimer = new Timer(async _ =>
            {
                await CheckDroneConnectionAsync();
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10)); // Проверка каждые 10 секунд

            return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
            _statusTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
    }
}

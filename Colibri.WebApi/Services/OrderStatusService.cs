
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Colibri.Data.Services.Abstracts;
using Colibri.WebApi.Services.Abstract;
using Microsoft.Extensions.Logging;

namespace Colibri.WebApi.Services;

public class OrderStatusService(ILogger<OrderStatusService> logger, IClientOrderService clientOrderService) : IOrderStatusService
{
    private readonly ConcurrentDictionary<WebSocket, byte> _connectedClients = new();
    private readonly ConcurrentDictionary<int, ConcurrentDictionary<WebSocket, byte>> _orderSubscriptions = new();
    private readonly ILogger<OrderStatusService> _logger = logger;
    private readonly IClientOrderService _clientOrderService = clientOrderService;

    public void AddConnection(WebSocket webSocket)
    {
        _connectedClients.TryAdd(webSocket, 0);
        _logger.LogInformation($"Добавлено соединение WebSocket. Всего соединений.: {_connectedClients.Count}");
    }

    public async Task GetOrderUpdatesAsync(WebSocket webSocket)
    {
        try
        {
            var orders = await _clientOrderService.GetOrdersAsync();
            var message = new
            {
                type = "order_updates",
                timestamp = DateTime.UtcNow,
                orders = orders.Select(o => new
                {
                    id = o.Id,
                    status = o.Status,
                    last_update = o.UpdatedAt
                })
            };

            await SendToWebSocketAsync(webSocket, message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при получении обновлений заказа.: {ex.Message}");
        }
    }

    public async Task NotifyAllClientsAsync()
    {
        try
        {
            var allOrders = await _clientOrderService.GetOrdersAsync();
            var message = new
            {
                type = "all_orders_status",
                timestamp = DateTime.UtcNow,
                orders = allOrders.Select(o => new
                {
                    id = o.Id,
                    status = o.Status,
                    created_at = o.CreatedAt,
                    updated_at = o.UpdatedAt
                })
            };

            await SendToAllAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при уведомлении всех клиентов: {ex.Message}");
        }

    }

    public async Task NotifyOrderUpdateAsync(int orderId, string status, object additionalData = null)
    {
        try
        {
            var order = await _clientOrderService.GetOrderByIdAsync(orderId);
            if (order == null) return;

            var message = new
            {
                type = "order_update",
                timestamp = DateTime.UtcNow,
                order = new
                {
                    id = order.Id,
                    status = status,
                    previous_status = order.Status,
                    created_at = order.CreatedAt,
                    updated_at = DateTime.UtcNow
                },
                additional_data = additionalData
            };

            // Обновляем статус в БД
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            // Здесь нужно вызвать метод обновления заказа, если он есть в IClientOrderService

            // Отправляем всем подписанным клиентам
            await SendToOrderSubscribersAsync(orderId, message);

            // Также отправляем всем подключенным клиентам (опционально)
            await SendToAllAsync(new
            {
                type = "order_status_changed",
                order_id = orderId,
                new_status = status,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Ошибка при уведомлении об обновлении заказа.");
        }
    }

    public void RemoveConnection(WebSocket webSocket)
    {
        _connectedClients.TryRemove(webSocket, out _);

        // Удаляем из всех подписок
        foreach (var subscription in _orderSubscriptions)
        {
            subscription.Value.TryRemove(webSocket, out _);
        }

        _logger.LogInformation($"Заказ на подключение WebSocket удален. Всего подключений.: {_connectedClients.Count}");
    }

    public async Task SubscribeToOrder(WebSocket webSocket, int orderId)
    {
        try
        {
            var subscriptions = _orderSubscriptions.GetOrAdd(orderId, _ => new ConcurrentDictionary<WebSocket, byte>());
            subscriptions.TryAdd(webSocket, 0);

            _logger.LogInformation($"Клиент оформил заказ {orderId}. Общее количество подписчиков: {subscriptions.Count}");

            // Отправляем текущий статус заказа
            var order = await _clientOrderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                var message = new
                {
                    type = "order_subscribed",
                    order_id = orderId,
                    current_status = order.Status,
                    timestamp = DateTime.UtcNow
                };

                await SendToWebSocketAsync(webSocket, message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error subscribing to order: {ex.Message}");
        }
    }

    public async Task UnsubscribeFromOrder(WebSocket webSocket, int orderId)
    {
        try
        {
            if (_orderSubscriptions.TryGetValue(orderId, out var subscriptions))
            {
                subscriptions.TryRemove(webSocket, out _);

                // Удаляем запись, если больше нет подписчиков
                if (subscriptions.IsEmpty)
                {
                    _orderSubscriptions.TryRemove(orderId, out _);
                }

                _logger.LogInformation($"Клиент отписался от заказа {orderId}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Ошибка при отписке от заказа: {ex.Message}");
        }
    }

    private async Task SendToWebSocketAsync(WebSocket webSocket, object message)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            try
            {
                var json = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);
                var segment = new ArraySegment<byte>(buffer);

                await webSocket.SendAsync(segment,
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

    private async Task SendToOrderSubscribersAsync(int orderId, object message)
    {
        if (_orderSubscriptions.TryGetValue(orderId, out var subscribers))
        {
            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);
            var segment = new ArraySegment<byte>(buffer);

            foreach (var socket in subscribers.Keys)
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
                        _logger.LogWarning($"Не удалось отправить заказ подписчику: {ex.Message}");
                    }
                }
            }
        }
    }

    private async Task SendToAllAsync(object message)
    {
        var json = JsonSerializer.Serialize(message);
        var buffer = Encoding.UTF8.GetBytes(json);
        var segment = new ArraySegment<byte>(buffer);

        foreach (var socket in _connectedClients.Keys)
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

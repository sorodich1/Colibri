using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Colibri.WebApi.WebSokets;

public class DroneWebSocketHandler
{
    private static readonly ConcurrentDictionary<string, WebSocket> _connection = new();
    private static readonly ConcurrentDictionary<string, string> _droneSubscriptions = new();

    public async Task HandleWebSocketConnection(HttpContext context)
    {
        Console.WriteLine($"🎯 WebSocket connection requested from: {context.Connection.RemoteIpAddress}");
        
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString();

        _connection[connectionId] = webSocket;
        Console.WriteLine($"✅ WebSocket подключен: {connectionId}, всего подключений: {_connection.Count}");

        try
        {
            await HandleWebSocketMessages(webSocket, connectionId);
        }
        finally
        {
            _connection.TryRemove(connectionId, out _);
            RemoveSubscription(connectionId);
            Console.WriteLine($"❌ WebSocket отключен: {connectionId}, осталось подключений: {_connection.Count}");
        }
    }

    private async Task HandleWebSocketMessages(WebSocket webSocket, string connectionId)
    {
        var buffer = new byte[1024 * 4];

        try
        {
            // Сразу отправляем приветственное сообщение
            await SendToConnection(webSocket, new { 
                type = "welcome", 
                message = "Connected to drone WebSocket",
                timestamp = DateTime.UtcNow
            });

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), 
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"📨 Получено от клиента {connectionId}: {message}");
                    await ProcessClientMessage(message, connectionId, webSocket);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine($"🔒 Клиент {connectionId} закрыл соединение");
                    break;
                }
            }
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"❌ WebSocket ошибка у {connectionId}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Общая ошибка у {connectionId}: {ex.Message}");
        }
    }

    private async Task ProcessClientMessage(string message, string connectionId, WebSocket webSocket)
    {
        try
        {
            Console.WriteLine($"📨 Получено от клиента {connectionId}: {message}");
            
            // Детальная десериализация с проверкой
            WebSocketMessage messageObj = null;
            try
            {
                messageObj = JsonSerializer.Deserialize<WebSocketMessage>(message);
                Console.WriteLine($"🔍 Десериализовано: Type='{messageObj?.type}', DroneId='{messageObj?.droneId}'");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"❌ Ошибка JSON десериализации: {jsonEx.Message}");
                Console.WriteLine($"🔍 JSON который не удалось разобрать: {message}");
                await SendToConnection(webSocket, new { type = "error", message = "Invalid JSON format" });
                return;
            }

            if (messageObj?.type == "subscribe")
            {
                var droneId = messageObj.droneId ?? "drone-1";
                Console.WriteLine($"🔍 Обработка подписки на дрона: {droneId}");
                
                _droneSubscriptions[connectionId] = droneId;
                
                Console.WriteLine($"✅ Клиент {connectionId} подписан на дрона: {droneId}, всего подписок: {_droneSubscriptions.Count}");
                
                // ОТПРАВЛЯЕМ ПОДТВЕРЖДЕНИЕ ПОДПИСКИ
                var response = new 
                { 
                    type = "subscribed", 
                    droneId = droneId,
                    message = "Successfully subscribed to drone updates",
                    timestamp = DateTime.UtcNow
                };
                
                Console.WriteLine($"📤 Пытаемся отправить подтверждение подписки...");
                await SendToConnection(webSocket, response);
                Console.WriteLine($"✅ Подтверждение подписки отправлено клиенту {connectionId}");
            }
            else if (messageObj?.type == "unsubscribe")
            {
                RemoveSubscription(connectionId);
                await SendToConnection(webSocket, new { type = "unsubscribed" });
            }
            else
            {
                Console.WriteLine($"⚠️ Неизвестный тип сообщения: {messageObj?.type}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ КРИТИЧЕСКАЯ ОШИБКА в ProcessClientMessage:");
            Console.WriteLine($"🔍 Сообщение: {ex.Message}");
            Console.WriteLine($"🔍 StackTrace: {ex.StackTrace}");
            
            try
            {
                await SendToConnection(webSocket, new { type = "error", message = ex.Message });
            }
            catch (Exception sendEx)
            {
                Console.WriteLine($"🔥 Не удалось отправить сообщение об ошибке: {sendEx.Message}");
            }
        }
    }

    private void RemoveSubscription(string connectionId)
    {
        _droneSubscriptions.TryRemove(connectionId, out _);
    }

    private async Task SendToConnection(WebSocket webSocket, object message)
    {
        if (webSocket.State == WebSocketState.Open)
        {
            try
            {
                var jsonMessage = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(jsonMessage);
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer), 
                    WebSocketMessageType.Text, 
                    true, 
                    CancellationToken.None);
                Console.WriteLine($"📤 Отправлено клиенту: {jsonMessage}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка отправки сообщения: {ex.Message}");
            }
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
            timestamp = DateTime.UtcNow
        };

        var tasks = new List<Task>();
        int sentCount = 0;

        Console.WriteLine($"🚀 Отправка статуса для дрона {droneId}, подписчиков: {_droneSubscriptions.Count}");

        foreach (var subscription in _droneSubscriptions)
        {
            if (subscription.Value == droneId && _connection.TryGetValue(subscription.Key, out var webSocket))
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    tasks.Add(SendToConnection(webSocket, message));
                    sentCount++;
                    Console.WriteLine($"📨 Отправка подключению: {subscription.Key}");
                }
                else
                {
                    Console.WriteLine($"⚠️ WebSocket {subscription.Key} не подключен");
                }
            }
        }

        if (tasks.Count > 0)
        {
            await Task.WhenAll(tasks);
            Console.WriteLine($"✅ Статус отправлен {sentCount} клиентам");
        }
        else
        {
            Console.WriteLine($"⚠️ Нет активных подписчиков для дрона {droneId}");
        }
    }

    public async Task SendTestStatus(string droneId, string status, string message)
    {
        var statusUpdate = new
        {
            status = status,
            message = message,
            timestamp = DateTime.UtcNow,
            isTest = true
        };

        await BroadcastDroneStatus(droneId, statusUpdate);
    }
}

public class WebSocketMessage
{
    public string type { get; set; }
    public string droneId { get; set; }
}
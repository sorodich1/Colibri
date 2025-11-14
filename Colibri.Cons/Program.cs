using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Colibri.Cons
{
    class Program
    {
        private static ClientWebSocket _webSocket;
        private static bool _isConnected = false;

        static async Task Main(string[] args)
        {
            Console.WriteLine("🔌 WebSocket Drone Status Tester");
            Console.WriteLine("=================================");
            
            var serverUrl = "ws://localhost:5000/ws/drone";
            var droneId = "drone-1";

            try
            {
                await ConnectToWebSocket(serverUrl, droneId);
                await ReceiveMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка: {ex.Message}");
                Console.WriteLine($"🔍 Детали: {ex}");
            }
            finally
            {
                await Disconnect();
            }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        static async Task ConnectToWebSocket(string url, string droneId)
        {
            _webSocket = new ClientWebSocket();
            
            Console.WriteLine($"🔄 Подключаемся к {url}...");
            
            // Добавляем таймаут подключения
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            
            try
            {
                await _webSocket.ConnectAsync(new Uri(url), cts.Token);
                _isConnected = true;
                Console.WriteLine("✅ Подключение установлено!");

                // Подписываемся на дрона
                var subscribeMessage = new
                {
                    type = "subscribe",
                    droneId = droneId
                };

                await SendMessage(JsonSerializer.Serialize(subscribeMessage));
                Console.WriteLine($"📨 Отправлена подписка на дрона: {droneId}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("❌ Таймаут подключения (5 секунд)");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка подключения: {ex.Message}");
                throw;
            }
        }

        static async Task SendMessage(string message)
        {
            if (!_isConnected || _webSocket.State != WebSocketState.Open)
            {
                Console.WriteLine("❌ WebSocket не подключен");
                return;
            }

            try
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка отправки сообщения: {ex.Message}");
            }
        }

        static async Task ReceiveMessages()
        {
            var buffer = new byte[4096];
            
            Console.WriteLine("\n🎯 Ожидаем сообщения от сервера...");
            Console.WriteLine("Нажмите 'q' для выхода\n");

            while (_isConnected && _webSocket.State == WebSocketState.Open)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                    {
                        Console.WriteLine("\n🛑 Завершение работы...");
                        break;
                    }
                }

                try
                {
                    // УБЕРИТЕ ТАЙМАУТ - используем CancellationToken.None
                    var result = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None); // ИЗМЕНЕНО: убран таймаут

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessMessage(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine("🔒 Сервер закрыл соединение");
                        break;
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine($"❌ WebSocket ошибка: {ex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Неожиданная ошибка: {ex.Message}");
                    break;
                }

                await Task.Delay(100);
            }
        }
        static void ProcessMessage(string jsonMessage)
        {
            try
            {
                Console.WriteLine($"\n📥 Получено сообщение: {jsonMessage}");

                using var document = JsonDocument.Parse(jsonMessage);
                var messageType = document.RootElement.GetProperty("type").GetString();
                
                switch (messageType)
                {
                    case "status_update":
                        var data = document.RootElement.GetProperty("data");
                        var status = data.GetProperty("status").GetString();
                        var message = data.GetProperty("message").GetString();
                        var timestamp = data.GetProperty("timestamp").GetString();
                        
                        Console.ForegroundColor = GetStatusColor(status);
                        Console.WriteLine($"🚁 СТАТУС ДРОНА: {status.ToUpper()}");
                        Console.ResetColor();
                        Console.WriteLine($"   📝 {message}");
                        Console.WriteLine($"   🕒 {timestamp}");
                        break;

                    case "subscribed":
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("✅ Успешно подписались на обновления дрона");
                        Console.ResetColor();
                        break;

                    case "unsubscribed":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("📴 Отписались от обновлений дрона");
                        Console.ResetColor();
                        break;

                    case "error":
                        var errorMessage = document.RootElement.GetProperty("message").GetString();
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"❌ ОШИБКА: {errorMessage}");
                        Console.ResetColor();
                        break;

                    default:
                        Console.WriteLine($"📨 Неизвестный тип сообщения: {messageType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Ошибка обработки сообщения: {ex.Message}");
            }
        }

        static ConsoleColor GetStatusColor(string status)
        {
            return status?.ToLower() switch
            {
                "preparing" => ConsoleColor.Yellow,
                "flying" => ConsoleColor.Green,
                "receiving_package" => ConsoleColor.Blue,
                "returning" => ConsoleColor.Cyan,
                "landed" => ConsoleColor.Magenta,
                "error" => ConsoleColor.Red,
                _ => ConsoleColor.White
            };
        }

        static async Task Disconnect()
        {
            if (_webSocket != null)
            {
                if (_webSocket.State == WebSocketState.Open)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Закрыто клиентом",
                        CancellationToken.None);
                }
                _webSocket.Dispose();
                _isConnected = false;
                Console.WriteLine("🔌 Соединение закрыто");
            }
        }
    }
}
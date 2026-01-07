using Colibri.WebApi.ConfigureService;
using Colibri.WebApi.Services.Abstract;
using Colibri.WebApi.WebSokets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Colibri.WebApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// ����� ����� � ����������
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            try
            {
                builder.Host.UseSerilog();

                Log.Information("������ ����������");

                BaseConfigure.Configuration(builder.Services, builder.Configuration);
                AuthConfigure.Configuration(builder.Services, builder.Configuration);
                CorsConfigure.Configuration(builder.Services, builder.Configuration);
                SwaggerConfigure.Configuration(builder.Services, builder.Configuration);
                TransientConfigure.Configuration(builder.Services, builder.Configuration);

                builder.Services.AddControllers();

                builder.Services.AddSingleton<DroneWebSocketHandler>();

                builder.Services.AddControllersWithViews();

                var app = builder.Build();

                app.UseStaticFiles();

                app.UseWebSockets();

                 app.Use(async (context, next) =>
                {
                    Console.WriteLine($"📨 Request: {context.Request.Method} {context.Request.Path}");
                    
                    if (context.Request.Path == "/ws/drone")
                    {
                        Console.WriteLine("🎯 WebSocket route matched!");
                        
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            Console.WriteLine("🔌 WebSocket request detected");
                            var webSocketHandler = context.RequestServices.GetRequiredService<DroneWebSocketHandler>();
                            await webSocketHandler.HandleWebSocketConnection(context);
                        }
                        else
                        {
                            Console.WriteLine("❌ Not a WebSocket request");
                            context.Response.StatusCode = 400;
                        }
                    }
                    else if (context.Request.Path == "/ws/status")
                    {
                        // Альтернативный эндпоинт только для статуса
                        Console.WriteLine("🎯 Status WebSocket route matched!");
                        
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            Console.WriteLine("🔌 Status WebSocket request detected");
                            
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var statusService = context.RequestServices.GetRequiredService<IWebSocketStatusService>();
                            
                            statusService.AddConnection(webSocket);
                            
                            // Отправляем текущий статус сразу после подключения
                            await statusService.CheckDroneConnectionAsync();
                            
                            // Держим соединение открытым (упрощенная версия)
                            await KeepConnectionOpen(webSocket, statusService);
                        }
                        else
                        {
                            Console.WriteLine("❌ Not a WebSocket request for status");
                            context.Response.StatusCode = 400;
                        }
                    }
                    else
                    {
                        await next();
                    }
                });

                app.UseRouting();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                app.UseSwagger(); // Без кастомных настроек
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                });

                app.Run();
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "���������� ����������� � �������");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static async Task KeepConnectionOpen(WebSocket webSocket, IWebSocketStatusService statusService)
        {
            var buffer = new byte[1024 * 4];
            
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    // Ждем сообщения от клиента (или закрытия соединения)
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

                    // Если клиент отправил сообщение, можно его обработать
                    if (result.MessageType == WebSocketMessageType.Text && result.Count > 0)
                    {
                        var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received from client: {message}");
                        
                        // Если клиент запросил статус
                        if (message.Contains("\"type\":\"get_status\"") || message.Contains("status"))
                        {
                            await statusService.CheckDroneConnectionAsync();
                        }
                    }
                }
            }
            catch (WebSocketException ex)
            {
                Console.WriteLine($"WebSocket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in KeepConnectionOpen: {ex.Message}");
            }
            finally
            {
                // Удаляем соединение из списка
                statusService.RemoveConnection(webSocket);
                webSocket?.Dispose();
                Console.WriteLine("WebSocket connection closed");
            }
        }
    }
}

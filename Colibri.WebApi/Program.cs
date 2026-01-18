using Colibri.WebApi.ConfigureService;
using Colibri.WebApi.Services.Abstract;
using Colibri.WebApi.WebSokets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Colibri.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration)
                .CreateLogger();

            try
            {
                builder.Host.UseSerilog();

                Log.Information("Starting application");

                BaseConfigure.Configuration(builder.Services, builder.Configuration);
                AuthConfigure.Configuration(builder.Services, builder.Configuration);
                CorsConfigure.Configuration(builder.Services, builder.Configuration);
                SwaggerConfigure.Configuration(builder.Services, builder.Configuration);
                TransientConfigure.Configuration(builder.Services, builder.Configuration);

                builder.Services.AddControllers();
                builder.Services.AddSingleton<DroneWebSocketHandler>();
                builder.Services.AddControllersWithViews();

                var app = builder.Build();
                
                // Критически важно: статические файлы должны быть первыми!
                // Явно указываем путь к wwwroot
                var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(webRootPath),
                    RequestPath = "",
                    ServeUnknownFileTypes = true,
                    DefaultContentType = "application/octet-stream"
                });
                
                Console.WriteLine($"Static files path: {webRootPath}");
                Console.WriteLine($"Directory exists: {Directory.Exists(webRootPath)}");
                
                // Проверяем, существует ли файл CSS
                var cssPath = Path.Combine(webRootPath, "css", "site.css");
                Console.WriteLine($"CSS path: {cssPath}");
                Console.WriteLine($"CSS exists: {File.Exists(cssPath)}");
                
                app.UseWebSockets();
                
                // WebSocket обработчики
                app.Use(async (context, next) =>
                {
                    var path = context.Request.Path.Value ?? "";
                    
                    // WebSocket для дрона
                    if (path == "/ws/drone")
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
                        return;
                    }
                    
                    // WebSocket для статуса
                    else if (path == "/ws/status")
                    {
                        Console.WriteLine("🎯 Status WebSocket route matched!");
                        
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            Console.WriteLine("🔌 Status WebSocket request detected");
                            
                            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                            var statusService = context.RequestServices.GetRequiredService<IWebSocketStatusService>();
                            
                            statusService.AddConnection(webSocket);
                            await statusService.CheckDroneConnectionAsync();
                            await KeepConnectionOpen(webSocket, statusService);
                        }
                        else
                        {
                            Console.WriteLine("❌ Not a WebSocket request for status");
                            context.Response.StatusCode = 400;
                        }
                        return;
                    }
                    
                    await next();
                });

                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                });

                app.Run();
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
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

                    if (result.MessageType == WebSocketMessageType.Text && result.Count > 0)
                    {
                        var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Console.WriteLine($"Received from client: {message}");
                        
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
                statusService.RemoveConnection(webSocket);
                webSocket?.Dispose();
                Console.WriteLine("WebSocket connection closed");
            }
        }
    }
}
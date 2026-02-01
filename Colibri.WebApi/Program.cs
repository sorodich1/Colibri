using Colibri.WebApi.ConfigureService;
using Colibri.WebApi.Services;
using Colibri.WebApi.Services.Abstract;
using Colibri.WebApi.WebSokets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Serilog;
using System;
using System.IO;

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

                // Регистрация WebSocket handlers
                builder.Services.AddSingleton<DroneWebSocketHandler>();
                builder.Services.AddSingleton<DroneBoxWebSocketHandler>();
                
                // Регистрация сервисов для дронбокса ПО НОВОЙ СТРУКТУРЕ
                builder.Services.AddSingleton<IPingService, PingService>();
                builder.Services.AddSingleton<IDroneBoxStatusService, DroneBoxStatusService>();
                
                // Фоновая задача для периодической проверки дронбокса
                builder.Services.AddHostedService<DroneBoxBackgroundService>();
                
                builder.Services.AddControllers();
                builder.Services.AddControllersWithViews();

                var app = builder.Build();
                
                // 1. WebSockets ДО static files
                app.UseWebSockets();
                
                // 2. WebSocket обработчики ДО static files
                app.Use(async (context, next) =>
                {
                    var path = context.Request.Path;
                    
                    if (path.StartsWithSegments("/ws"))
                    {
                        Console.WriteLine($"🎯 WebSocket request to: {path}");
                        
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            Console.WriteLine("🔌 WebSocket request detected");
                            
                            if (path == "/ws/status")
                            {
                                // Для дрона
                                var handler = context.RequestServices.GetRequiredService<DroneWebSocketHandler>();
                                await handler.HandleWebSocketConnection(context);
                                return;
                            }
                            else if (path == "/ws/statusdb")
                            {
                                // Для дронбокса
                                var handler = context.RequestServices.GetRequiredService<DroneBoxWebSocketHandler>();
                                await handler.HandleWebSocketConnection(context);
                                return;
                            }
                            else
                            {
                                Console.WriteLine($"❌ Unknown WebSocket path: {path}");
                                context.Response.StatusCode = 404;
                                return;
                            }
                        }
                        else
                        {
                            Console.WriteLine("❌ Not a WebSocket request");
                            context.Response.StatusCode = 400;
                            return;
                        }
                    }
                    
                    await next();
                });

                // 3. Static files ПОСЛЕ WebSocket
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

                // 4. Routing и остальное
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
    }
}
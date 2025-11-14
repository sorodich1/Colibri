using Colibri.WebApi.ConfigureService;
using Colibri.WebApi.WebSokets;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;

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

                var app = builder.Build();

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
    }
}

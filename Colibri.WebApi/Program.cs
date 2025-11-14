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

                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();

                app.Use(async (context, next) =>
                {
                    if (context.Request.Path == "/ws/drone")
                    {
                        if (context.WebSockets.IsWebSocketRequest)
                        {
                            var webSocketHandler = context.RequestServices.GetRequiredService<DroneWebSocketHandler>();
                            await webSocketHandler.HandleWebSocketConnection(context);
                        }
                        else
                        {
                            context.Response.StatusCode = 400;
                        }
                    }
                    else
                    {
                        await next();
                    }
                });

                app.MapControllers();

                app.UseSwagger(c =>
                {
                    c.RouteTemplate = "swagger/{documentName}/swagger.json";
                });

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Microservice API 1.2.1");
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

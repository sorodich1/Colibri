using Colibri.WebApi.ConfigureService;
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
        /// Точка входа в приложение
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

                Log.Information("Запуск приложения");

                BaseConfigure.Configuration(builder.Services, builder.Configuration);
                AuthConfigure.Configuration(builder.Services, builder.Configuration);
                CorsConfigure.Configuration(builder.Services, builder.Configuration);
                SwaggerConfigure.Configuration(builder.Services, builder.Configuration);
                TransientConfigure.Configuration(builder.Services, builder.Configuration);

                builder.Services.AddControllers();

                var app = builder.Build();

                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllers();

                app.UseSwagger();

                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint($"/swagger/{builder.Configuration["Swagger:Version"]}/swagger.json", "Mobile API V1");
                    c.RoutePrefix = string.Empty;
                });

                //using (var scope = app.Services.CreateScope())
                //{
                //    var context = scope.ServiceProvider.GetRequiredService<ILoggerService>();
                //    try
                //    {
                //        context.LogMessage(null, "Подключение установлено", Microsoft.Extensions.Logging.LogLevel.Information);
                //    }
                //    catch (Exception ex)
                //    {
                //        Console.WriteLine($"Ошибка при подключении к базе данных: {ex}");
                //    }
                //}

                app.Run();
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Приложение завершилось с ошибкой");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}

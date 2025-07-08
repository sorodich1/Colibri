using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Colibri.WebApi.ConfigureService
{
    /// <summary>
    /// Класс для настройки CORS (Cross-Origin Resource Sharing).
    /// </summary>
    public class CorsConfigure
    {
        /// <summary>
        ///  Метод настройки CORS с использованием предоставленных параметров конфигурации.
        /// </summary>
        /// <param name="services">Коллекция служб для настройки.</param>
        /// <param name="configuration">Объект конфигурации для доступа к настройкам.</param>
        public static void Configuration(IServiceCollection services, IConfiguration configuration)
        {
            var origins = configuration["Cors:Origins"].Split(',');

            // Получение списка допустимых источников из конфигурации и разделение их на массив
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    if (origins != null && origins.Length > 0)
                    {
                        if (origins.Contains("*"))
                        {
                            builder.AllowAnyHeader();
                            builder.AllowAnyMethod();
                            builder.SetIsOriginAllowed(host => true);
                            builder.AllowCredentials();
                        }
                        else
                        {
                            foreach (var origin in origins)
                            {
                                builder.WithOrigins(origin);
                            }
                        }
                    }
                });
            });
        }
    }
}

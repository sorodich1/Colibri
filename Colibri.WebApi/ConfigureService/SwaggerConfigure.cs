using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace Colibri.WebApi.ConfigureService
{
    /// <summary>
    /// Класс для настройки Swagger для API документации.
    /// </summary>
    public class SwaggerConfigure
    {
        /// <summary>
        /// Метод настройки Swagger с использованием предоставленных параметров 
        /// </summary>
        /// <param name="services">Коллекция служб для настройки.</param>
        /// <param name="configuration">Объект конфигурации для доступа к настройкам.</param>
        public static void Configuration(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(configuration["Swagger:Version"], new OpenApiInfo { Title = configuration["Swagger:Title"], Version = configuration["Swagger:Version"] });


                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = configuration["Swagger:Description"],
                    Name = configuration["Swagger:Name"],
                    Type = SecuritySchemeType.ApiKey
                });

                // Определение документа Swagger с использованием информации из конфигурации
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }
    }
}

using Colibri.Data.Context;
using Colibri.Data.Entity;
using Colibri.WebApi.Extensions;
using Colibri.WebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Colibri.WebApi.ConfigureService
{
    /// <summary>
    /// Класс для базовой настройки служб приложения.
    /// </summary>
    public class BaseConfigure
    {
        /// <summary>
        /// Метод настройки служб с использованием предоставленных параметров конфигурации.
        /// </summary>
        /// <param name="services">Коллекция служб для настройки.</param>
        /// <param name="configuration">Объект конфигурации для доступа к настройкам.</param>
        public static void Configuration(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextPool<AppDbContext>(config =>
            {
                config.UseMySql(configuration["Project:ConnectionString"], new MySqlServerVersion(new Version(9, 0, 0)));
            });

            services.AddIdentity<User, Role>()
                .AddUserStore<AppUserStore>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddRouting(options => options.LowercaseUrls = true);

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 0;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                options.User.AllowedUserNameCharacters = null;
                options.User.RequireUniqueEmail = false;
            });


            services.AddControllers();
            services.AddMemoryCache();
            services.AddRouting();

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddOptions();
            services.Configure<CurrentAppSettings>(configuration.GetSection(nameof(CurrentAppSettings)));
            services.Configure<MvcOptions>(options => options.UseRouteSlugify());
            services.AddLocalization();
            services.AddHttpContextAccessor();
            services.AddResponseCaching();
        }
    }
}
